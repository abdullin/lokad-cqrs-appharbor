#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.IO;

namespace SaaS.TapeStorage
{
    /// <summary>
    /// <para>Persists records in a tape stream, using SHA1 hashing and "magic" number sequences
    /// to detect corruption and offer partial recovery.</para>
    /// <para>System information is written in such a way, that if data is Unicode human-readable,
    /// then the file will be human-readable as well.</para>
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class FileTapeStream : ITapeStream
    {
        readonly FileInfo _data;

        public FileTapeStream(string name)
        {
            _data = new FileInfo(name);
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

            if (!_data.Exists)
            {
                // file could've been created since the last check
                _data.Refresh();
                if (!_data.Exists)
                {
                    yield break;
                }
            }

            using (var file = OpenForRead())
            {
                if (!TapeStreamSerializer.SkipRecords(afterVersion, file))
                    yield break;

                for (var i = 0; i < maxCount; i++)
                {
                    if (file.Position == file.Length)
                        yield break;

                    var record = TapeStreamSerializer.ReadRecord(file);

                    yield return record;
                }
            }
        }

        public long GetCurrentVersion()
        {
            try
            {
                using (var s = OpenForRead())
                {
                    // seek end
                    s.Seek(0, SeekOrigin.End);
                    return TapeStreamSerializer.ReadVersionFromTheEnd(s);
                }
            }
            catch (FileNotFoundException)
            {
                return 0;
            }
            catch (DirectoryNotFoundException)
            {
                return 0;
            }
        }

        public long TryAppend(byte[] buffer, TapeAppendCondition condition)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            if (buffer.Length == 0)
                throw new ArgumentException("Buffer must contain at least one byte.");

            using (var file = OpenForWrite())
            {
                // we need to know version first.
                file.Seek(0, SeekOrigin.End);
                var version = TapeStreamSerializer.ReadVersionFromTheEnd(file);

                if (!condition.Satisfy(version))
                    return 0;

                var versionToWrite = version + 1;
                TapeStreamSerializer.WriteRecord(file, buffer, versionToWrite);

                return versionToWrite;
            }
        }

        public void AppendNonAtomic(IEnumerable<TapeRecord> records)
        {
            if (records == null)
                throw new ArgumentNullException("records");

            // if enumeration is lazy, then this would cause another enum
            //if (!records.Any())
            //    return;

            using (var file = OpenForWrite())
            {
                file.Seek(0, SeekOrigin.End);

                foreach (var record in records)
                {
                    if (record.Data.Length == 0)
                        throw new ArgumentException("Record must contain at least one byte.");

                    var versionToWrite = record.Version;
                    TapeStreamSerializer.WriteRecord(file, record.Data, versionToWrite);
                }
            }
        }

        FileStream OpenForWrite()
        {
            // we allow concurrent reading
            // no more writers are allowed
            return _data.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
        }

        FileStream OpenForRead()
        {
            // we allow concurrent writing or reading
            return _data.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        }
    }
}