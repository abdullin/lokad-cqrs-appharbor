using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.TapeStorage
{
    public class BlockBlobTapeStream : ITapeStream
    {
        const int MaxBlockSize = 500 * 1024;
        const int FourMb = 4 * 1024 * 1024;

        readonly CloudBlobContainer _container;
        readonly string _blobName;

        public BlockBlobTapeStream(CloudBlobContainer container, string name)
        {
            _container = container;
            _blobName = name;
        }

        public IEnumerable<TapeRecord> ReadRecords(long afterVersion, int maxCount)
        {
            if (afterVersion < 0)
                throw new ArgumentOutOfRangeException("afterVersion", "Must be zero or greater.");

            if (maxCount <= 0)
                throw new ArgumentOutOfRangeException("maxCount", "Must be more than zero.");

            // afterVersion + maxCount > long.MaxValue, but transformed to avoid overflow
            if (afterVersion > long.MaxValue - maxCount)
                throw new ArgumentOutOfRangeException("maxCount", "Version will exceed long.MaxValue.");

            var blob = _container.GetBlockBlobReference(_blobName);

            var dataExists = blob.Exists();

            // we return empty result if writer didn't even start writing to the storage.
            if (!dataExists)
                return Enumerable.Empty<TapeRecord>();

            var blockList = blob.DownloadBlockList();

            var range = GetReadRange(blockList, afterVersion, maxCount);

            if (range.Item4 == 0)
                return new TapeRecord[0];

            using (var s = blob.OpenRead())
            {
                s.ReadAheadSize = range.Item2;
                s.Seek(range.Item1, SeekOrigin.Begin);

                using (var reader = new BinaryReader(s))
                {
                    var bytes = reader.ReadBytes(range.Item2);

                    using (var ms = new MemoryStream(bytes))
                    {
                        TapeStreamSerializer.SkipRecords(range.Item3, ms);
                        var records = Enumerable.Range(0, range.Item4)
                            .Select(i => TapeStreamSerializer.ReadRecord(ms))
                            .ToArray();

                        return records;
                    }
                }
            }
        }

        public long GetCurrentVersion()
        {
            var blob = _container.GetBlockBlobReference(_blobName);

            if (!blob.Exists())
                return 0;

            var lastBlock = blob.DownloadBlockList().LastOrDefault();

            if (default(ListBlockItem) == lastBlock)
                return 0;

            var nameInfo = Naming.GetInfo(DecodeName(lastBlock.Name));
            var version = nameInfo.FirstVersion - 1 + nameInfo.Count;

            return version;
        }

        public long TryAppend(byte[] buffer, TapeAppendCondition appendCondition = new TapeAppendCondition())
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            if (buffer.Length == 0)
                throw new ArgumentException("Buffer must contain at least one byte.");

            long version;
            int lastBlockSize;
            long offset;
            long firstVersion;
            long count;
            List<string> blockNames;

            var blob = _container.GetBlockBlobReference(_blobName);
            if (blob.Exists())
            {
                var blockList = blob.DownloadBlockList().ToArray();
                blockNames = blockList.Select(bl => bl.Name).ToList();
                var lastBlock = blockList.LastOrDefault();

                if (default(ListBlockItem) == lastBlock)
                {
                    version = 0;
                    lastBlockSize = int.MaxValue;
                    offset = 0;
                    firstVersion = 1;
                    count = 0;
                }
                else
                {
                    var nameInfo = Naming.GetInfo(DecodeName(lastBlock.Name));
                    firstVersion = nameInfo.FirstVersion;
                    version = nameInfo.FirstVersion - 1 + nameInfo.Count;
                    count = nameInfo.Count;

                    if (lastBlock.Size > int.MaxValue)
                        throw new InvalidOperationException("last block size must be in 'int' range");

                    lastBlockSize = (int) lastBlock.Size;
                    offset = blockList.Reverse().Skip(1).Sum(l => l.Size);
                }
            }
            else
            {
                version = 0;
                lastBlockSize = int.MaxValue;
                offset = 0;
                firstVersion = 1;
                count = 0;
                blockNames = new List<string>();
            }

            if (!appendCondition.Satisfy(version))
                return 0;

            if (version > long.MaxValue - 1)
                throw new IndexOutOfRangeException("Version is more than long.MaxValue.");

            if (buffer.Length > FourMb)
                throw new ArgumentException("buffer size must be less than or equal to 4 Mb", "buffer");

            using (var outStream = new MemoryStream())
            {
                if (buffer.Length < MaxBlockSize && lastBlockSize <= MaxBlockSize - buffer.Length)
                {
                    // read old block
                    using (var s = blob.OpenRead())
                    {
                        s.Seek(offset, SeekOrigin.Begin);
                        s.CopyTo(outStream);
                        TapeStreamSerializer.WriteRecord(outStream, buffer, version + 1);

                        count++;
                        blockNames.RemoveAt(blockNames.Count - 1);
                    }
                }
                else
                {
                    TapeStreamSerializer.WriteRecord(outStream, buffer, version + 1);

                    firstVersion = version + 1;
                    count = 1;
                }

                var blockId = EncodeName(Naming.GetName(firstVersion, count));

                string md5Hash;
                outStream.Seek(0, SeekOrigin.Begin);
                using (var md5 = MD5.Create()) {
                    md5Hash = Convert.ToBase64String(md5.ComputeHash(outStream));
                }

                outStream.Seek(0, SeekOrigin.Begin);
                blob.PutBlock(blockId, outStream, md5Hash);
                blockNames.Add(blockId);
                blob.PutBlockList(blockNames);

                return version+1;
            }
        }

        static Tuple<long, int, int, int> GetReadRange(IEnumerable<ListBlockItem> blockList, long afterVersion, int maxCount)
        {
            const int notSet = -1;

            long offset = 0;
            long size = 0;
            var skipRecs = 0;
            long readRecs = notSet;

            var blockInfos = blockList
                .Select(i => new {item = i, info = Naming.GetInfo(DecodeName(i.Name))})
                .ToArray();

            if (blockInfos.Length == 0)
                return Tuple.Create(0L, 0, 0, 0);

            foreach (var a in blockInfos)
            {
                if (readRecs == notSet)
                {
                    // start aggregating if we find a block containing records we need
                    var containsAfterVersion = a.info.FirstVersion + a.info.Count - 1 > afterVersion;
                    if (containsAfterVersion)
                    {
                        skipRecs = (int)(afterVersion + 1 - a.info.FirstVersion);
                        readRecs = (int)(a.info.Count - skipRecs);
                        size = a.item.Size;

                        if (readRecs >= maxCount)
                        {
                            readRecs = maxCount;
                            break;
                        }
                    }
                    else
                    {
                        offset += a.item.Size;
                    }
                }
                else
                {
                    size += a.item.Size;

                    if (readRecs > maxCount - a.info.Count)
                    {
                        // we found maxCount records
                        readRecs = maxCount;
                        break;
                    }
                    readRecs += a.info.Count;
                }
            }

            if (size > int.MaxValue)
                throw new NotSupportedException(string.Format("Reading more than {0} bytes not supported.", int.MaxValue));
            if (readRecs > int.MaxValue)
                throw new NotSupportedException(string.Format("Reading more than {0} records not supported.", int.MaxValue));

            if (readRecs == notSet)
                readRecs = 0;

            return Tuple.Create(offset, (int) size, skipRecs, (int) readRecs);
        }

        static string EncodeName(string name)
        {
            return Convert.ToBase64String(Encoding.ASCII.GetBytes(name));
        }

        static string DecodeName(string name)
        {
            return Encoding.ASCII.GetString(Convert.FromBase64String(name));
        }

        static class Naming
        {
            const char Separator = '-';

            public static NameInfo GetInfo(string name)
            {
                var parts = name.Split(Separator);

                if (parts.Length != 2)
                    throw new InvalidOperationException("Name of BlockBlobListItem must consist of two parts. Are you sure that this block blob was created by BlockBlobTapeStream?");

                long firstVersion;
                if (!long.TryParse(parts[0], out firstVersion))
                    throw new InvalidOperationException("Can not parse 'version' part of name of BlockBlobListItem. Are you sure that this block blob was created by BlockBlobTapeStream?");

                long count;
                if (!long.TryParse(parts[1], out count))
                    throw new InvalidOperationException("Can not parse 'count' part of name of BlockBlobListItem. Are you sure that this block blob was created by BlockBlobTapeStream?");

                return new NameInfo
                    {
                        FirstVersion = firstVersion,
                        Count = count
                    };
            }

            public static string GetName(long firstVersion, long count)
            {
                return string.Format("{0:d10}-{1:d10}", firstVersion, count);
            }
        }

        struct NameInfo
        {
            public long FirstVersion;
            public long Count;
        }
    }
}