#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Collections.Concurrent;

namespace SaaS.Partition
{
    public sealed class MemoryQueueWriter : IQueueWriter
    {
        readonly BlockingCollection<byte[]> _queue;

        public string Name { get; private set; }

        public MemoryQueueWriter(BlockingCollection<byte[]> queue, string name)
        {
            _queue = queue;
            Name = name;
        }

        public void PutMessage(byte[] envelope)
        {
            _queue.Add(envelope);
        }

        public void Init() {}
    }
}