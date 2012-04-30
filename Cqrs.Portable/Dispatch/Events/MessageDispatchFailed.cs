#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using SaaS.Partition;

namespace SaaS.Dispatch.Events
{
    [Serializable]
    public sealed class MessageDispatchFailed : ISystemEvent
    {
        public Exception Exception { get; private set; }
        public MessageTransportContext Message { get; private set; }
        public string QueueName { get; private set; }

        public MessageDispatchFailed(MessageTransportContext message, string queueName, Exception exception)
        {
            Exception = exception;
            Message = message;
            QueueName = queueName;
        }

        public override string ToString()
        {
            return string.Format("Failed to consume {0} from '{1}': {2}", Message.TransportMessage, QueueName,
                Exception.Message);
        }
    }
}