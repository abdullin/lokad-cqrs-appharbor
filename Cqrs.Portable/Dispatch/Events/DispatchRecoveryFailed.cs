#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using SaaS.Partition;

namespace SaaS.Dispatch.Events
{
    [Serializable]
    public sealed class DispatchRecoveryFailed : ISystemEvent
    {
        public Exception DispatchException { get; private set; }
        public MessageTransportContext Message { get; private set; }
        public string QueueName { get; private set; }

        public DispatchRecoveryFailed(Exception exception, MessageTransportContext message, string queueName)
        {
            DispatchException = exception;
            Message = message;
            QueueName = queueName;
        }

        public override string ToString()
        {
            return string.Format("Failed to recover dispatch '{0}' from '{1}': {2}", Message.TransportMessage, QueueName,
                DispatchException.Message);
        }
    }
}