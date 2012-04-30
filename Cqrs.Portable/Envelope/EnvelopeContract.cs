#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Runtime.Serialization;

namespace Lokad.Cqrs.Envelope
{
    [DataContract(Namespace = "Lokad.Cqrs.v2", Name = "Envelope"), Serializable]
    public sealed class EnvelopeContract
    {
        [DataMember(Order = 1)]
        public string EnvelopeId { get; private set; }

        [DataMember(Order = 2)]
        public EnvelopeAttributeContract[] EnvelopeAttributes { get; private set; }

        [DataMember(Order = 3)]
        public MessageContract[] Messages { get; private set; }

        [DataMember(Order = 4)]
        public DateTime DeliverOnUtc { get; set; }

        [DataMember(Order = 5)]
        public DateTime CreatedOnUtc { get; set; }


        public EnvelopeContract(string envelopeId, EnvelopeAttributeContract[] envelopeAttributes,
            MessageContract[] messages,
            DateTime deliverOnUtc, DateTime createdOnUtc)
        {
            EnvelopeId = envelopeId;
            DeliverOnUtc = deliverOnUtc;
            EnvelopeAttributes = envelopeAttributes;
            Messages = messages;
            CreatedOnUtc = createdOnUtc;
        }

// ReSharper disable UnusedMember.Local
        EnvelopeContract()
// ReSharper restore UnusedMember.Local
        {
            Messages = NoMessages;
            EnvelopeAttributes = NoAttributes;
        }

        static readonly MessageContract[] NoMessages = new MessageContract[0];
        static readonly EnvelopeAttributeContract[] NoAttributes = new EnvelopeAttributeContract[0];
    }
}