﻿#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using Lokad.Cqrs.Core.Envelope.Scenarios;
using Lokad.Cqrs.Envelope;
using NUnit.Framework;

namespace Lokad.Cqrs.Core.Envelope
{
    [TestFixture]
    public sealed class Play_all_for_DataContracts : When_envelope_is_serialized
    {
        readonly IEnvelopeStreamer _streamer = BuildStreamer(new EnvelopeSerializerWithDataContracts());
        protected override ImmutableEnvelope RoundtripViaSerializer(EnvelopeBuilder builder)
        {
            var bytes = _streamer.SaveEnvelopeData(builder.Build());
            return _streamer.ReadAsEnvelopeData(bytes);
        }
    }

    [TestFixture]
    public sealed class Play_all_for_ServiceStackJSON : When_envelope_is_serialized
    {
        readonly IEnvelopeStreamer _streamer = BuildStreamer(new EnvelopeSerializerWithServiceStackJSON());
        protected override ImmutableEnvelope RoundtripViaSerializer(EnvelopeBuilder builder)
        {
            var bytes = _streamer.SaveEnvelopeData(builder.Build());
            return _streamer.ReadAsEnvelopeData(bytes);
        }
    }
}