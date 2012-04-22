#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Lokad.Cqrs.TapeStorage
{
    /// <summary>
    /// Named tape stream, that usually matches to an aggregate instance
    /// </summary>
    public interface ITapeStream
    {
        /// <summary>
        /// Reads up to <see cref="maxCount"/> records starting with version next to <see cref="afterVersion"/>.
        /// </summary>
        /// <param name="afterVersion">Number of version to start after.</param>
        /// <param name="maxCount">The max number of records to load.</param>
        /// <returns>collection of blocks</returns>
        IEnumerable<TapeRecord> ReadRecords(long afterVersion, int maxCount);

        /// <summary>
        /// Returns current storage version
        /// </summary>
        /// <returns>current version of the storage</returns>
        long GetCurrentVersion();

        /// <summary>
        /// Tries the append data to the tape storage, ensuring that
        /// the version condition is met (if the condition is specified).
        /// </summary>
        /// <param name="buffer">The data to append.</param>
        /// <param name="appendCondition">The append condition.</param>
        /// <returns>version of the appended data</returns>
        long TryAppend(byte[] buffer, TapeAppendCondition appendCondition = default(TapeAppendCondition));
    }

    /// <summary>
    /// Contains information about the committed data
    /// </summary>
    public sealed class TapeRecord
    {
        public readonly long Version;
        public readonly byte[] Data;

        public TapeRecord(long version, byte[] data)
        {
            Version = version;
            Data = data;
        }
    }

    public sealed class MemoryTapeContainer : ITapeContainer
    {
        ConcurrentDictionary<string, IList<TapeRecord>> _dict = new ConcurrentDictionary<string, IList<TapeRecord>>();


        public ITapeStream GetOrCreateStream(string name)
        {
            return new MemoryTapeStream(_dict, name);
        }

        public void InitializeForWriting() {}
    }

    sealed class MemoryTapeStream : ITapeStream
    {
        ConcurrentDictionary<string, IList<TapeRecord>> _dictionary;
        readonly string _name;

        public MemoryTapeStream(ConcurrentDictionary<string, IList<TapeRecord>> dictionary, string name)
        {
            _dictionary = dictionary;
            _name = name;
        }

        public IEnumerable<TapeRecord> ReadRecords(long afterVersion, int maxCount)
        {
            if (afterVersion < 0)
                throw new ArgumentOutOfRangeException("afterVersion", "Must be zero or greater.");

            if (maxCount <= 0)
                throw new ArgumentOutOfRangeException("maxCount", "Must be more than zero.");

            IList<TapeRecord> bytes;
            if (_dictionary.TryGetValue(_name, out bytes))
            {
                foreach (var bytese in bytes.Where(r => r.Version > afterVersion).Take(maxCount))
                {
                    yield return bytese;
                }
            }
        }

        public long GetCurrentVersion()
        {
            IList<TapeRecord> records;
            if (_dictionary.TryGetValue(_name, out records))
            {
                return records.Count;
            }
            return 0;
        }

        public long TryAppend(byte[] buffer, TapeAppendCondition appendCondition = new TapeAppendCondition())
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            if (buffer.Length == 0)
                throw new ArgumentException("Buffer must contain at least one byte.");

            try
            {
                var result = _dictionary.AddOrUpdate(_name, s =>
                    {
                        appendCondition.Enforce(0);
                        var records = new List<TapeRecord>();
                        records.Add(new TapeRecord(1, buffer));
                        return records;
                    }, (s, list) =>
                        {
                            appendCondition.Enforce(list.Count);
                            return list.Concat(new[] {new TapeRecord(list.Count + 1, buffer)}).ToList();
                        });

                return result.Count;
            }
            catch (TapeAppendConditionException e)
            {
                return 0;
            }
        }
    }
}