#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using Lokad.Cqrs.Envelope;
using NUnit.Framework;

// ReSharper disable InconsistentNaming

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    [TestFixture]
    public sealed class Given_Atomic_Scenarios_When_Memory : Given_Atomic_Scenarios
    {
        

        protected override Setup ConfigureComponents(IEnvelopeStreamer streamer)
        {
            var _config = new MemoryStorageConfig();
            return new Setup
            {
                Store = _config.CreateNuclear(new TestStrategy()),
                Inbox = _config.CreateInbox("dev"),
                Sender = _config.CreateSimpleSender(streamer, "dev")

            };
        }
    }
}