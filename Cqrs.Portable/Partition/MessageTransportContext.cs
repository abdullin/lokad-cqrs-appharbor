#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

namespace SaaS.Partition
{
    /// <summary>
    /// Describes retrieved message along with the queue name and some transport info.
    /// </summary>
    /// <remarks>It is used to send ACK/NACK back to the originating queue.</remarks>
    public sealed class MessageTransportContext
    {
        public readonly object TransportMessage;
        public readonly byte[] Unpacked;
        public readonly string QueueName;

        public MessageTransportContext(object transportMessage, byte[] unpacked, string queueName)
        {
            TransportMessage = transportMessage;
            QueueName = queueName;
            Unpacked = unpacked;
        }
    }
}