#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;

namespace Lokad.Cqrs.Envelope.Events
{
    [Serializable]
    public sealed class EnvelopeDuplicateDiscarded : ISystemEvent
    {
        public string EnvelopeId { get; private set; }

        public EnvelopeDuplicateDiscarded(string envelopeId)
        {
            EnvelopeId = envelopeId;
        }

        public override string ToString()
        {
            return string.Format("[{0}] duplicate discarded", EnvelopeId);
        }
    }
}