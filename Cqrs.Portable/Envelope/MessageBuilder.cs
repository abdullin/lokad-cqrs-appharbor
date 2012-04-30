﻿#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;

namespace Lokad.Cqrs.Envelope
{
    public sealed class MessageBuilder : HideObjectMembersFromIntelliSense
    {
        internal readonly IDictionary<string, string> Attributes = new Dictionary<string, string>();
        public readonly Type MappedType;
        public readonly object Content;

        public MessageBuilder(Type mappedType, object content)
        {
            MappedType = mappedType;
            Content = content;
        }

        public void AddAttribute(string key, string value)
        {
            Attributes.Add(key, value);
        }
    }
}