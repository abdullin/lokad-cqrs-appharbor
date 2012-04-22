#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;

namespace Lokad.Cqrs.Envelope.Events
{
    [Serializable]
    public sealed class EnvelopeDispatched : ISystemEvent
    {
        public ImmutableEnvelope Envelope { get; private set; }
        public string Dispatcher { get; private set; }

        public EnvelopeDispatched(ImmutableEnvelope envelope, string dispatcher)
        {
            Envelope = envelope;
            Dispatcher = dispatcher;
        }

        public override string ToString()
        {
            return string.Format("Envelope '{0}' was dispatched by '{1}'", Envelope.EnvelopeId, Dispatcher);
        }
    }
}