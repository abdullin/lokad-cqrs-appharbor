#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace SaaS.Envelope
{
    public sealed class EnvelopeSerializerWithDataContracts : IEnvelopeSerializer
    {
        readonly DataContractSerializer _serializer;

        public EnvelopeSerializerWithDataContracts()
        {
            _serializer = new DataContractSerializer(typeof(EnvelopeContract));
        }

        public void SerializeEnvelope(Stream stream, EnvelopeContract c)
        {
            //using (var compressed = destination.Compress(true))
            using (var writer = XmlDictionaryWriter.CreateBinaryWriter(stream, null, null, false))
            {
                _serializer.WriteObject(writer, c);
            }
        }

        public EnvelopeContract DeserializeEnvelope(Stream stream)
        {
            using (var reader = XmlDictionaryReader.CreateBinaryReader(stream, XmlDictionaryReaderQuotas.Max))
            {
                return (EnvelopeContract) _serializer.ReadObject(reader);
            }
        }
    }
}