#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using Lokad.Cqrs.Envelope;
using Lokad.Cqrs.Feature.AtomicStorage;
using NUnit.Framework;

namespace Lokad.Cqrs.Synthetic
{
    [TestFixture]
    public sealed class Given_Tx_Scenarios_When_Memory : Given_Tx_Scenarios
    {
        protected override Setup ComposeComponents(IEnvelopeStreamer streamer)
        {
            var config = new MemoryStorageConfig();
            return new Setup
            {
                Inbox = config.CreateInbox("in"),
                Sender = config.CreateSimpleSender(streamer, "in"),
                Storage = config.CreateNuclear(new TestStrategy())
            };
        }
    }
}