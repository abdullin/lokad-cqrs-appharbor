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
    public sealed class Given_Basic_Scenarios_When_Files : Given_Basic_Scenarios
    {
        readonly FileStorageConfig _config;

        public Given_Basic_Scenarios_When_Files()
        {
            _config = FileStorage.CreateConfig(typeof(Given_Basic_Scenarios_When_Files).Name);
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

        protected override Setup ConfigureComponents(IEnvelopeStreamer config)
        {
            return new Setup
                {
                    Store = _config.CreateNuclear(new TestStrategy()),
                    Sender = _config.CreateSimpleSender(config, "my-queue"),
                    Inbox = _config.CreateInbox("my-queue")
                };
        }
    }
}