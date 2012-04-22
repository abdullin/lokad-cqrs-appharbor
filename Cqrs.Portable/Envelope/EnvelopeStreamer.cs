#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

//using ProtoBuf;

namespace Lokad.Cqrs.Envelope
{
    public sealed class EnvelopeStreamer : IEnvelopeStreamer
    {
        readonly IEnvelopeSerializer _envelopeSerializer;
        readonly IDataSerializer _dataSerializer;

        public EnvelopeStreamer(IDataSerializer dataSerializer, IEnvelopeSerializer envelopeSerializer = null)
        {
            _envelopeSerializer = envelopeSerializer ?? new EnvelopeSerializerWithDataContracts();
            _dataSerializer = dataSerializer;
        }

        public static EnvelopeStreamer CreateDefault(params Type[] types)
        {
            return new EnvelopeStreamer(new DataSerializerWithDataContracts(types),
                new EnvelopeSerializerWithDataContracts());
        }

        public static EnvelopeStreamer CreateDefault(IEnumerable<Type> types)
        {
            return CreateDefault(types.Where(t => !t.IsAbstract).ToArray());
        }

        public byte[] SaveEnvelopeData(ImmutableEnvelope envelope)
        {
            //  string contract, Guid messageId, Uri sender, 
            var itemContracts = new MessageContract[envelope.Items.Length];
            using (var content = new MemoryStream())
            {
                int position = 0;
                for (int i = 0; i < envelope.Items.Length; i++)
                {
                    var item = envelope.Items[i];

                    string name;
                    if (!_dataSerializer.TryGetContractNameByType(item.MappedType, out name))
                    {
                        var error = string.Format("Failed to find contract name to serialize '{0}'.", item.MappedType);
                        throw new InvalidOperationException(error);
                    }
                    // normal serializers have a nasty habbit of closing the stream after they are done
                    // we can suppress that or use a wrapper now instead
                    using (var itemStream = new MemoryStream())
                    {
                        _dataSerializer.Serialize(item.Content, item.MappedType, itemStream);
                        var bytes = itemStream.ToArray();
                        content.Write(bytes, 0, bytes.Length);
                    }


                    int size = (int) content.Position - position;
                    var attribContracts = EnvelopeConvert.ItemAttributesToContract(item.GetAllAttributes());
                    itemContracts[i] = new MessageContract(name, size, position, attribContracts);

                    position += size;
                }

                var envelopeAttribs = EnvelopeConvert.EnvelopeAttributesToContract(envelope.GetAllAttributes());


                var contract = new EnvelopeContract(envelope.EnvelopeId, envelopeAttribs, itemContracts,
                    envelope.DeliverOnUtc, envelope.CreatedOnUtc);

                using (var stream = new MemoryStream())
                {
                    // skip header
                    stream.Seek(EnvelopeHeaderContract.FixedSize, SeekOrigin.Begin);
                    // save envelope attributes
                    _envelopeSerializer.SerializeEnvelope(stream, contract);
                    var envelopeBytes = stream.Position - EnvelopeHeaderContract.FixedSize;
                    // copy data
                    content.WriteTo(stream);
                    // write the header
                    stream.Seek(0, SeekOrigin.Begin);
                    var header = new EnvelopeHeaderContract(EnvelopeHeaderContract.Schema2DataFormat, envelopeBytes, 0);
                    header.WriteToStream(stream);
                    return stream.ToArray();
                }
            }
        }


        public ImmutableEnvelope ReadAsEnvelopeData(byte[] buffer)
        {
            var header = EnvelopeHeaderContract.ReadHeader(buffer);

            if (header.MessageFormatVersion != EnvelopeHeaderContract.Schema2DataFormat)
                throw new InvalidOperationException("Unexpected bytes in enveloper header");


            EnvelopeContract envelope;
            using (var stream = new MemoryStream(buffer, EnvelopeHeaderContract.FixedSize, (int) header.EnvelopeBytes))
            {
                envelope = _envelopeSerializer.DeserializeEnvelope(stream);
            }

            var items = new ImmutableMessage[envelope.Messages.Length];

            for (var i = 0; i < items.Length; i++)
            {
                var itemContract = envelope.Messages[i];
                var attributes = EnvelopeConvert.ItemAttributesFromContract(itemContract.Attributes);
                Type contractType;

                var itemPosition = EnvelopeHeaderContract.FixedSize + (int) header.EnvelopeBytes +
                    (int) itemContract.ContentPosition;
                var itemSize = (int) itemContract.ContentSize;
                if (_dataSerializer.TryGetContractTypeByName(itemContract.ContractName, out contractType))
                {
                    // we can deserialize. Convert it to a message
                    using (var stream = new MemoryStream(buffer, itemPosition, itemSize))
                    {
                        var instance = _dataSerializer.Deserialize(stream, contractType);
                        items[i] = new ImmutableMessage(contractType, instance, attributes, i);
                    }
                }
                else
                {
                    // we can't deserialize. Keep it as buffer
                    var bufferInstance = new byte[itemContract.ContentSize];
                    Buffer.BlockCopy(buffer, itemPosition, bufferInstance, 0, itemSize);
                    items[i] = new ImmutableMessage(null, bufferInstance, attributes, i);
                }
            }

            var envelopeAttributes = EnvelopeConvert.EnvelopeAttributesFromContract(envelope.EnvelopeAttributes);
            return new ImmutableEnvelope(envelope.EnvelopeId, envelopeAttributes, items, envelope.DeliverOnUtc,
                envelope.CreatedOnUtc);
        }
    }
}