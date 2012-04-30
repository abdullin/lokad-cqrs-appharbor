#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Collections.Concurrent;
using System.Threading;

namespace SaaS.Partition
{
    /// <summary>
    /// In-memory implementation of <see cref="IPartitionInbox"/> that uses concurrency primitives
    /// </summary>
    public sealed class MemoryPartitionInbox : IPartitionInbox
    {
        readonly BlockingCollection<byte[]>[] _queues;
        readonly string[] _names;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryPartitionInbox"/> class.
        /// </summary>
        /// <param name="queues">The queues.</param>
        /// <param name="names">Names for these queues.</param>
        public MemoryPartitionInbox(BlockingCollection<byte[]>[] queues, string[] names)
        {
            _queues = queues;
            _names = names;
        }


        public void InitIfNeeded() {}

        public void AckMessage(MessageTransportContext message) {}

        public bool TakeMessage(CancellationToken token, out MessageTransportContext context)
        {
            while (!token.IsCancellationRequested)
            {
                // if incoming message is delayed and in future -> push it to the timer queue.
                // timer will be responsible for publishing back.

                byte[] envelope;
                var result = BlockingCollection<byte[]>.TakeFromAny(_queues, out envelope);
                if (result >= 0)
                {
                    context = new MessageTransportContext(result, envelope, _names[result]);
                    return true;
                }
            }
            context = null;
            return false;
        }

        public void TryNotifyNack(MessageTransportContext context)
        {
            var id = (int) context.TransportMessage;

            _queues[id].Add(context.Unpacked);
        }
    }
}