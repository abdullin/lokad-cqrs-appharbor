#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Threading;
using System.Linq;
using Lokad.Cqrs.Partition;

namespace Lokad.Cqrs.Feature.AzurePartition.Inbox
{
    /// <summary>
    /// Polling implementation of message reciever for Azure queues
    /// </summary>
    public sealed class AzurePartitionInbox : IPartitionInbox
    {
        readonly StatelessAzureQueueReader[] _readers;
        readonly Func<uint, TimeSpan> _waiter;
        uint _emptyCycles;

        readonly string _name;

        public override string ToString()
        {
            return _name;
        }

        public AzurePartitionInbox(StatelessAzureQueueReader[] readers,
            Func<uint, TimeSpan> waiter)
        {
            _readers = readers;
            _waiter = waiter;

            _name = string.Format("Azure Inbox [{0}]", string.Join(",", readers.Select(r => r.Name).ToArray()));
        }


        public void InitIfNeeded()
        {
            foreach (var x in _readers)
            {
                x.InitIfNeeded();
            }
        }

        public void AckMessage(MessageTransportContext message)
        {
            foreach (var queue in _readers)
            {
                if (queue.Name == message.QueueName)
                {
                    queue.AckMessage(message);
                }
            }
        }

        public void TryNotifyNack(MessageTransportContext context)
        {
        }

        public bool TakeMessage(CancellationToken token, out MessageTransportContext context)
        {
            while (!token.IsCancellationRequested)
            {
                for (var i = 0; i < _readers.Length; i++)
                {
                    var queue = _readers[i];

                    var message = queue.TryGetMessage();
                    switch (message.State)
                    {
                        case GetEnvelopeResultState.Success:

                            _emptyCycles = 0;
                            context = message.Message;
                            return true;
                        case GetEnvelopeResultState.Empty:
                            _emptyCycles += 1;
                            break;
                        case GetEnvelopeResultState.Exception:
                            // access problem, fall back a bit
                            break;
                        case GetEnvelopeResultState.Retry:
                            // this could be the poison
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    var waiting = _waiter(_emptyCycles);
                    token.WaitHandle.WaitOne(waiting);
                }
            }
            context = null;
            return false;
        }
    }
}