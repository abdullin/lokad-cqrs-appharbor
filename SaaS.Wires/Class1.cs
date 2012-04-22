#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using Lokad.Cqrs;
using Lokad.Cqrs.Envelope;
using Lokad.Cqrs.Evil;

namespace SaaS.Wires
{
    public static class Contracts
    {
        static Type[] LoadMessageContracts()
        {
            //var messages = new[] { typeof(CustomerCreated), typeof(UserIndexLookup) }
            //    .SelectMany(t => t.Assembly.GetExportedTypes())
            //    .Where(t => typeof(IHubMessage).IsAssignableFrom(t))
            //    .Where(t => !t.IsAbstract)
            //    .ToArray();
            //return messages;
            throw new NotImplementedException();
        }

        public static IEnvelopeStreamer CreateStreamer()
        {
            return new EnvelopeStreamer(new DataSerializer(LoadMessageContracts()), new EnvelopeSerializer());
        }

        sealed class EnvelopeSerializer : IEnvelopeSerializer
        {
            public void SerializeEnvelope(Stream stream, EnvelopeContract contract)
            {
                throw new NotImplementedException();
            }

            public EnvelopeContract DeserializeEnvelope(Stream stream)
            {
                throw new NotImplementedException();
                //return Serializer.Deserialize<EnvelopeContract>(stream);
            }
        }

        class DataSerializer : AbstractDataSerializer
        {
            public DataSerializer(ICollection<Type> knownTypes)
                : base(knownTypes)
            {
                throw new NotImplementedException();
                //RuntimeTypeModel.Default[typeof(DateTimeOffset)].Add("m_dateTime", "m_offsetMinutes");
            }

            protected override Formatter PrepareFormatter(Type type)
            {
                var name = ContractEvil.GetContractReference(type);
                throw new NotImplementedException();
                //var formatter = RuntimeTypeModel.Default.CreateFormatter(type);
                //return new Formatter(name, formatter.Deserialize, (o, stream) => formatter.Serialize(stream, o));
            }
        }
    }
}