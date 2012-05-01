#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using System;
using Lokad.Cqrs.Feature.AtomicStorage;
using NUnit.Framework;

// ReSharper disable InconsistentNaming

namespace Lokad.Cqrs.AtomicStorage
{
    [TestFixture]
    public sealed class Given_Atomic_Scenarios_When_Azure : Given_Atomic_Scenarios
    {
        protected override Setup ConfigureComponents(IEnvelopeStreamer streamer)
        {
            TestSpeed = 7000;

            var dev = AzureStorage.CreateConfigurationForDev();
            WipeAzureAccount.Fast(s => s.StartsWith("test-"), dev);
            return new Setup
                {
                    Store = dev.CreateNuclear(new TestStrategy()),
                    Inbox = dev.CreateInbox("test-incoming", visibilityTimeout : TimeSpan.FromSeconds(1)),
                    Sender = dev.CreateSimpleSender(streamer, "test-incoming")
                };
        }
    }
    [TestFixture]
    public sealed class Given_Atomic_Storage_When_Azure : Given_Atomic_Storage
    {
        protected override NuclearStorage Compose(IDocumentStrategy strategy)
        {
            var dev = AzureStorage.CreateConfigurationForDev();
            WipeAzureAccount.Fast(s => s.StartsWith("test-views"), dev);
            return dev.CreateNuclear(strategy);
        }
    }
}