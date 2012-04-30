#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;

namespace SaaS.Envelope.Events
{
    [Serializable]
    public sealed class EnvelopeQuarantined : ISystemEvent
    {
        public Exception LastException { get; private set; }
        public string Dispatcher { get; private set; }
        public ImmutableEnvelope Envelope { get; private set; }

        public EnvelopeQuarantined(Exception lastException, string dispatcher, ImmutableEnvelope envelope)
        {
            LastException = lastException;
            Dispatcher = dispatcher;
            Envelope = envelope;
        }

        public override string ToString()
        {
            return string.Format("Quarantined '{0}': {1}", Envelope.EnvelopeId, LastException.Message);
        }
    }

    [Serializable]
    public sealed class EnvelopeCleanupFailed : ISystemEvent
    {
        public Exception Exception { get; private set; }
        public string Dispatcher { get; private set; }
        public ImmutableEnvelope Envelope { get; private set; }

        public EnvelopeCleanupFailed(Exception exception, string dispatcher, ImmutableEnvelope envelope)
        {
            Exception = exception;
            Dispatcher = dispatcher;
            Envelope = envelope;
        }
    }
}