#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using SaaS.Partition;

namespace SaaS.Dispatch.Events
{
    [Serializable]
    public sealed class MessageAcked : ISystemEvent
    {
        public MessageTransportContext Context { get; private set; }

        public MessageAcked(MessageTransportContext attributes)
        {
            Context = attributes;
        }

        public override string ToString()
        {
            return string.Format("[{0}] acked at '{1}'", Context.TransportMessage, Context.QueueName);
        }
    }

    [Serializable]
    public sealed class MessageInboxFailed : ISystemEvent
    {
        public Exception Exception { get; private set; }
        public string InboxName { get; private set; }
        public string MessageId { get; private set; }

        public MessageInboxFailed(Exception exception, string inboxName, string messageId)
        {
            Exception = exception;
            InboxName = inboxName;
            MessageId = messageId;
        }

        public override string ToString()
        {
            return string.Format("Failed to retrieve message from {0}: {1}.", InboxName, Exception.Message);
        }
    }
}