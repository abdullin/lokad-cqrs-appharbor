﻿#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace Lokad.Cqrs.Envelope
{
    static class EnvelopeConvert
    {
        public static ImmutableAttribute[] EnvelopeAttributesFromContract(
            ICollection<EnvelopeAttributeContract> attributes)
        {
            var list = new ImmutableAttribute[attributes.Count];

            var idx = 0;
            foreach (var attribute in attributes)
            {
                switch (attribute.Type)
                {
                    case EnvelopeAttributeTypeContract.Sender:
                        list[idx] = new ImmutableAttribute(MessageAttributes.EnvelopeSender, attribute.Value);
                        break;
                    case EnvelopeAttributeTypeContract.CustomString:
                        list[idx] = new ImmutableAttribute(attribute.Name, attribute.Value);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                idx += 1;
            }
            return list;
        }

        public static ImmutableAttribute[] ItemAttributesFromContract(ICollection<MessageAttributeContract> attributes)
        {
            return attributes
                .Select(attribute => new ImmutableAttribute(attribute.Name, attribute.Value))
                .ToArray();
        }

        public static MessageAttributeContract[] ItemAttributesToContract(
            ICollection<ImmutableAttribute> attributes)
        {
            var contracts = new MessageAttributeContract[attributes.Count];
            var pos = 0;

            foreach (var attrib in attributes)
            {
                switch (attrib.Key)
                {
                    default:
                        contracts[pos] = ItemAttributeValueToContract(attrib.Key, attrib.Value);
                        break;
                }

                pos += 1;
            }

            return contracts;
        }

        static MessageAttributeContract ItemAttributeValueToContract(string name, string value)
        {
            return new MessageAttributeContract
                {
                    Name = name,
                    Value = value
                };
        }

        public static EnvelopeAttributeContract[] EnvelopeAttributesToContract(
            ICollection<ImmutableAttribute> attributes)
        {
            var contracts = new EnvelopeAttributeContract[attributes.Count];
            int pos = 0;

            foreach (var attrib in attributes)
            {
                switch (attrib.Key)
                {
                    case MessageAttributes.EnvelopeSender:
                        contracts[pos] = new EnvelopeAttributeContract
                            {
                                Type = EnvelopeAttributeTypeContract.Sender,
                                Value = attrib.Value
                            };
                        break;
                    default:
                        contracts[pos] = new EnvelopeAttributeContract
                            {
                                Type = EnvelopeAttributeTypeContract.CustomString,
                                Name = attrib.Key,
                                Value = attrib.Value
                            };
                        break;
                }
                pos += 1;
            }

            return contracts;
        }
    }
}