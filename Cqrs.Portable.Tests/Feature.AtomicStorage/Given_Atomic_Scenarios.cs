#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using Lokad.Cqrs.AtomicStorage;
using Lokad.Cqrs.Build;
using Lokad.Cqrs.Core.Envelope;
using Lokad.Cqrs.Envelope;
using Lokad.Cqrs.Partition;
using NUnit.Framework;

// ReSharper disable InconsistentNaming

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public abstract class Given_Atomic_Scenarios
    {
        public sealed class Setup
        {
            public NuclearStorage Store;
            public SimpleMessageSender Sender;
            public IPartitionInbox Inbox;
        }

        readonly IEnvelopeStreamer _streamer = EnvelopeStreamer
            .CreateDefault(typeof(AtomicMessage), typeof(NuclearMessage));

       


        [Test]
        public void Then_typed_singleton_should_be_accessable_from_handler()
        {
            var setup = ConfigureComponents(_streamer);
            var builder = BootstrapHandlers(setup);

            using (var source = new CancellationTokenSource())
            using (Cancel_when_ok_received(source))
            {
                using (var engine = builder.Build())
                {
                    setup.Sender.SendOne(new AtomicMessage());
                    var task = engine.Start(source.Token);
                    task.Wait(TestSpeed);
                    Assert.IsTrue(source.IsCancellationRequested);
                }
            }
        }

        CqrsEngineBuilder BootstrapHandlers(Setup setup)
        {
            var builder = new CqrsEngineBuilder(_streamer);
            var writer = setup.Store.Container.GetWriter<unit, int>();
            var handler = new RedirectToCommand();
            handler.WireToLambda<AtomicMessage>(am => HandleAtomic(am, setup.Sender, writer));
            handler.WireToLambda<NuclearMessage>(am => HandleNuclear(am, setup.Sender, setup.Store));
            builder.Handle(setup.Inbox, envelope =>
            {
                foreach (var message in envelope.Items)
                {
                    handler.Invoke(message.Content);
                }
            });
            return builder;
        }

        [Test]
        public void Then_nuclear_storage_should_be_available()
        {
            var setup = ConfigureComponents(_streamer);
            var builder = BootstrapHandlers(setup);

            using (var source = new CancellationTokenSource())
            using (Cancel_when_ok_received(source))
            {
                using (var engine = builder.Build())
                {
                    setup.Sender.SendOne(new NuclearMessage());
                    var task = engine.Start(source.Token);
                    task.Wait(TestSpeed);
                    Assert.IsTrue(source.IsCancellationRequested);
                }
            }
        }

        protected abstract Setup ConfigureComponents(IEnvelopeStreamer streamer);

        [DataContract]
        public sealed class AtomicMessage {}

        [DataContract]
        public sealed class NuclearMessage {}

        static IDisposable Cancel_when_ok_received(CancellationTokenSource source)
        {
            return TestObserver.When<EnvelopeSent>(s =>
                {
                    if (s.Attributes.Any(a => a.Key == "ok"))
                    {
                        source.Cancel();
                    }
                });
        }

        protected static void HandleAtomic(AtomicMessage msg, SimpleMessageSender sender, IDocumentWriter<unit, int> arg3)
        {
            var count = arg3.AddOrUpdate(unit.it, () => 1, i => i + 1);
            if (count > 2)
            {
                sender.SendBatch(new object[] {}, e => e.AddString("ok"));
                return;
            }
            sender.SendOne(new AtomicMessage());
        }

        protected static void HandleNuclear(NuclearMessage msg, SimpleMessageSender sender, NuclearStorage storage)
        {
            var count = storage.AddOrUpdateSingleton(() => 1, i => i + 1);
            if (count >= 2)
            {
                sender.SendBatch(new object[] {}, e => e.AddString("ok"));
                return;
            }
            sender.SendOne(new NuclearMessage());
        }

        protected int TestSpeed = 2000;
    }
}