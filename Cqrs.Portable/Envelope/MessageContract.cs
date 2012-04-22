#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Runtime.Serialization;

namespace Lokad.Cqrs.Envelope
{
    [DataContract(Namespace = "Lokad.Cqrs.v2", Name = "Message"), Serializable]
    public sealed class MessageContract
    {
        [DataMember(Order = 1)]
        public string ContractName { get; private set; }

        [DataMember(Order = 2)]
        public long ContentSize { get; private set; }

        [DataMember(Order = 3)]
        public long ContentPosition { get; private set; }

        [DataMember(Order = 4)]
        public MessageAttributeContract[] Attributes { get; private set; }

        MessageContract()
        {
            Attributes = Empty;
        }

        public MessageContract(string contractName, long contentSize, long contentPosition,
            MessageAttributeContract[] attributes)
        {
            ContractName = contractName;
            ContentSize = contentSize;
            ContentPosition = contentPosition;
            Attributes = attributes;
        }

        static readonly MessageAttributeContract[] Empty = new MessageAttributeContract[0];
    }
}