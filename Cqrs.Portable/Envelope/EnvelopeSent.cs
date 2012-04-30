#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;

namespace Lokad.Cqrs.Envelope
{
    /// <summary>
    /// Is published whenever an event is sent.
    /// </summary>
    [Serializable]
    public sealed class EnvelopeSent : ISystemEvent
    {
        public readonly string QueueName;
        public readonly string EnvelopeId;
        public readonly string[] MappedTypes;
        public readonly ICollection<ImmutableAttribute> Attributes;

        public EnvelopeSent(string queueName, string envelopeId, string[] mappedTypes,
            ICollection<ImmutableAttribute> attributes)
        {
            QueueName = queueName;
            EnvelopeId = envelopeId;
            MappedTypes = mappedTypes;
            Attributes = attributes;
        }

        public override string ToString()
        {
            return string.Format("Sent {0} to '{1}' as [{2}]",
                string.Join("+", MappedTypes),
                QueueName,
                EnvelopeId);
        }
    }
}