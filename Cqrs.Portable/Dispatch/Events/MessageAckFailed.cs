#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Lokad.Cqrs.Partition;

namespace Lokad.Cqrs.Dispatch.Events
{
    [Serializable]
    public sealed class MessageAckFailed : ISystemEvent
    {
        public Exception Exception { get; private set; }
        public MessageTransportContext Context { get; private set; }

        public MessageAckFailed(Exception exception, MessageTransportContext context)
        {
            Exception = exception;
            Context = context;
        }

        public override string ToString()
        {
            return string.Format("Failed to ack '{0}' from '{1}': {2}", Context.TransportMessage, Context.QueueName,
                Exception.Message);
        }
    }
}