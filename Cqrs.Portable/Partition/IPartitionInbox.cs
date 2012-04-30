#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Threading;

namespace Lokad.Cqrs.Partition
{
    /// <summary>
    /// Retrieves (waiting if needed) message from one or more queues
    /// </summary>
    public interface IPartitionInbox
    {
        void InitIfNeeded();

        /// <summary>
        /// Acks the message (removing it from the original queue).
        /// </summary>
        /// <param name="message">The envelope.</param>
        void AckMessage(MessageTransportContext message);

        /// <summary>
        /// Tries to take the message, waiting for it, if needed
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="context">The context for the retrieved message.</param>
        /// <returns><em>true</em> if the message was retrieved, <em>false</em> otherwise</returns>
        bool TakeMessage(CancellationToken token, out MessageTransportContext context);

        /// <summary>
        /// Tries the notify the original queue that the message was not processed.
        /// </summary>
        /// <param name="context">The context of the message.</param>
        void TryNotifyNack(MessageTransportContext context);
    }
}