#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Lokad.Cqrs;
using Lokad.Cqrs.AtomicStorage;
using Lokad.Cqrs.Envelope;
using Lokad.Cqrs.Evil;
using ProtoBuf;
using ProtoBuf.Meta;
using ServiceStack.Text;

namespace SaaS.Wires
{
    public static class Contracts
    {
        static Type[] LoadMessageContracts()
        {
            var messages = new[] { typeof(InstanceStarted) }
                .SelectMany(t => t.Assembly.GetExportedTypes())
                .Where(t => typeof(ISampleMessage).IsAssignableFrom(t))
                .Where(t => !t.IsAbstract)
                .ToArray();
            return messages;
            
        }

        public static IEnvelopeStreamer CreateStreamer()
        {
            return new EnvelopeStreamer(new DataSerializer(LoadMessageContracts()), new EnvelopeSerializer());
        }

        sealed class EnvelopeSerializer : IEnvelopeSerializer
        {
            static byte[] NL = Encoding.UTF8.GetBytes(Environment.NewLine);
            public void SerializeEnvelope(Stream stream, EnvelopeContract contract)
            {
                stream.Write(NL,0,NL.Length);
                JsonSerializer.SerializeToStream(contract, stream);
                stream.Write(NL, 0, NL.Length);
            }

            public EnvelopeContract DeserializeEnvelope(Stream stream)
            {
                return JsonSerializer.DeserializeFromStream<EnvelopeContract>(stream);
            }
        }

        class DataSerializer : AbstractDataSerializer
        {
            public DataSerializer(ICollection<Type> knownTypes)
                : base(knownTypes)
            {
                RuntimeTypeModel.Default[typeof(DateTimeOffset)].Add("m_dateTime", "m_offsetMinutes");
            }

            protected override Formatter PrepareFormatter(Type type)
            {
                var name = ContractEvil.GetContractReference(type);
                return new Formatter(name, s => JsonSerializer.DeserializeFromStream(type, s), (o,s) =>
                    {
                        using(var writer = new StreamWriter(s))
                        {
                            writer.WriteLine();
                            writer.WriteLine(JsvFormatter.Format(JsonSerializer.SerializeToString(o, type)));
                        }
                        
                    });
                //var formatter = RuntimeTypeModel.Default.CreateFormatter(type);
                //return new Formatter(name, formatter.Deserialize, (o, stream) => formatter.Serialize(stream, o));
            }
        }
    }


    public sealed class DocumentStrategy : IDocumentStrategy
    {
        public string GetEntityBucket<T>()
        {
            return "doc-" + typeof(T).Name.ToLowerInvariant();
        }

        public string GetEntityLocation(Type entity, object key)
        {
            if (key is unit)
                return entity.Name.ToLowerInvariant() + ".txt";
            if (key is IIdentity)
                return IdentityConvert.ToStream((IIdentity)key) + ".txt";
            return key.ToString().ToLowerInvariant() + ".txt";
        }

        public void Serialize<TEntity>(TEntity entity, Stream stream)
        {
            var s = JsonSerializer.SerializeToString(entity);
            s = JsvFormatter.Format(s);

            using (var writer = new StreamWriter(stream))
            {
                writer.Write(s);
            }
        }

        public TEntity Deserialize<TEntity>(Stream stream)
        {
            return JsonSerializer.DeserializeFromStream<TEntity>(stream);
        }
    }
}