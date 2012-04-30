#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using Lokad.Cqrs.Envelope;
using Lokad.Cqrs.Feature.AtomicStorage;
using NUnit.Framework;

// ReSharper disable InconsistentNaming

namespace Lokad.Cqrs.Synthetic
{
    [TestFixture]
    public sealed class Given_Tx_Scenarios_When_Files : Given_Tx_Scenarios
    {
        readonly FileStorageConfig _config;

        public Given_Tx_Scenarios_When_Files()
        {
            _config = FileStorage.CreateConfig(GetType().Name);
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            _config.Wipe();
        }

        [SetUp]
        public void SetUp()
        {
            _config.Reset();
        }

        protected override Setup ComposeComponents(IEnvelopeStreamer streamer)
        {
            return new Setup
                {
                    Inbox = _config.CreateInbox("in"),
                    Sender = _config.CreateSimpleSender(streamer, "in"),
                    Storage = _config.CreateNuclear(new TestStrategy())
                };
        }
    }
}