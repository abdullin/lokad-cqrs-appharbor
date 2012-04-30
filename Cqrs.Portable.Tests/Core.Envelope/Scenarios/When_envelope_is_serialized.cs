#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Lokad.Cqrs.Envelope;
using NUnit.Framework;

namespace Lokad.Cqrs.Core.Envelope.Scenarios
{
    // ReSharper disable UnusedMember.Global
    // ReSharper disable InconsistentNaming
    // ReSharper disable DoNotCallOverridableMethodsInConstructor

    public abstract class When_envelope_is_serialized
    {
        protected abstract ImmutableEnvelope RoundtripViaSerializer(EnvelopeBuilder builder);

        protected static IEnvelopeStreamer BuildStreamer(IEnvelopeSerializer serializer)
        {
            return new EnvelopeStreamer(new DataSerializerWithBinaryFormatter(), serializer);
        }

        [Test]
        public void Empty_roundtrip_should_work()
        {
            var builder = new EnvelopeBuilder("my-id");
            var envelope = RoundtripViaSerializer(builder);
            Assert.AreEqual(envelope.EnvelopeId, "my-id");
        }

        [Test]
        public void Envelope_attributes_should_be_present()
        {
            var dateTime = DateTime.UtcNow;
            var time = RoundToMs(dateTime);
            var builder = new EnvelopeBuilder("my-id");
            builder.OverrideCreatedOnUtc(time);
            builder.AddString("Custom", "1");


            var envelope = RoundtripViaSerializer(builder);

            Assert.AreEqual("1", envelope.GetAttribute("Custom"));
            Assert.GreaterOrEqual(RoundToMs(envelope.CreatedOnUtc), time, "start time");
            var now = RoundToMs(dateTime);
            Assert.LessOrEqual(RoundToMs(envelope.CreatedOnUtc), now, "now");
        }

        public static DateTime RoundToMs(DateTime d)
        {
            return new DateTime(d.Year, d.Month, d.Day, d.Hour, d.Minute, d.Second);
        }

        [Test]
        public void Payload_should_be_serialized()
        {
            var builder = new EnvelopeBuilder("my-id");
            builder.AddItem(new MyMessage("42"));

            var envelope = RoundtripViaSerializer(builder);

            Assert.AreEqual(1, envelope.Items.Length);
            Assert.AreEqual("42", ((MyMessage) envelope.Items[0].Content).Value);
        }

        [Test]
        public void Multiple_payloads_are_handled_in_sequence()
        {
            var builder = new EnvelopeBuilder("my-id");

            for (int i = 0; i < 5; i++)
            {
                var content = new string('*', i);
                var added = builder.AddItem(new MyMessage(content));

                added.AddAttribute("hum", i.ToString());
            }

            var envelope = RoundtripViaSerializer(builder);

            Assert.AreEqual(5, envelope.Items.Length);

            for (int i = 0; i < 5; i++)
            {
                var messageItem = envelope.Items[i];
                Assert.AreEqual(new string('*', i), ((MyMessage) messageItem.Content).Value);
                Assert.AreEqual(i.ToString(), messageItem.GetAttribute("hum", ""));
            }
        }

        [Serializable]
        sealed class MyMessage
        {
            public readonly string Value;

            public MyMessage(string value)
            {
                Value = value;
            }

            public override string ToString()
            {
                return Value;
            }
        }
    }
}