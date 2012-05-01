#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using System;
using Lokad.Cqrs.Envelope;
using Lokad.Cqrs.Feature.AtomicStorage;
using NUnit.Framework;

// ReSharper disable InconsistentNaming

namespace Lokad.Cqrs.Synthetic
{
    [TestFixture]
    public sealed class Given_Tx_Scenarios_When_Composite_Azure : Given_Tx_Scenarios
    {
        protected override Setup ComposeComponents(IEnvelopeStreamer streamer)
        {
            // Azure dev is implemented via WS on top of SQL on top of FS.
            // this can be slow. And it will be
            TestSpeed = 7000;

            var dev = AzureStorage.CreateConfigurationForDev();
            WipeAzureAccount.Fast(s => s.StartsWith("test-"), dev);

            return new Setup
                {
                    Sender = dev.CreateSimpleSender(streamer, "test-incoming"),
                    Inbox = dev.CreateInbox("test-incoming", visibilityTimeout : TimeSpan.FromMilliseconds(1)),
                    Storage = dev.CreateNuclear(new TestStrategy())
                };
        }
    }
}