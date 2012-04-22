﻿#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Runtime.Serialization;

namespace Lokad.Cqrs.Envelope
{
    [DataContract(Namespace = "Lokad.Cqrs.v2", Name = "MessageAttribute"), Serializable]
    public sealed class MessageAttributeContract
    {
        [DataMember(Order = 2)]
        public string Name { get; set; }

        [DataMember(Order = 3)]
        public string Value { get; set; }
    }
}