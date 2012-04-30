#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using Lokad.Cqrs.Envelope;
using Lokad.Cqrs.Feature.AtomicStorage;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

namespace Lokad.Cqrs.Synthetic
{
    [TestFixture]
    public sealed class Given_Basic_Scenarios_When_Memory : Given_Basic_Scenarios
    {
        protected override Setup ConfigureComponents(IEnvelopeStreamer config)
        {
            var acc = new MemoryStorageConfig();
            return new Setup
                {
                    Store = acc.CreateNuclear(new TestStrategy()),
                    Inbox = acc.CreateInbox("queue"),
                    Sender = acc.CreateSimpleSender(config, "queue")
                };
        }
    }
}