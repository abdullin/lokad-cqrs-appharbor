#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.IO;
using Lokad.Cqrs.Core.Envelope.Scenarios;
using NUnit.Framework;
using ProtoBuf;

namespace Lokad.Cqrs.Envelope
{
    // ReSharper disable InconsistentNaming

    [TestFixture]
    public sealed class Play_all_for_ProtoBuf : When_envelope_is_serialized
    {

        public sealed class EnvelopeSerializerWithProtoBuf : IEnvelopeSerializer
        {
            public void SerializeEnvelope(Stream stream, EnvelopeContract c)
            {
                Serializer.Serialize(stream, c);
            }

            public EnvelopeContract DeserializeEnvelope(Stream stream)
            {
                return Serializer.Deserialize<EnvelopeContract>(stream);
            }
        }

        readonly IEnvelopeStreamer _streamer = BuildStreamer(new EnvelopeSerializerWithProtoBuf());

             
        protected override ImmutableEnvelope RoundtripViaSerializer(EnvelopeBuilder builder)
        {
            var bytes = _streamer.SaveEnvelopeData(builder.Build());
            return _streamer.ReadAsEnvelopeData(bytes);
        }
    }
}