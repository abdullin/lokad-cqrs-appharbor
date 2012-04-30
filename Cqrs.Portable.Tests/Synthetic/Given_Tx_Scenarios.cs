#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading;
using System.Transactions;
using Lokad.Cqrs.AtomicStorage;
using Lokad.Cqrs.Build;
using Lokad.Cqrs.Core;
using Lokad.Cqrs.Envelope;
using Lokad.Cqrs.Envelope.Events;
using Lokad.Cqrs.Feature.AtomicStorage;
using Lokad.Cqrs.Partition;
using NUnit.Framework;
using System.Linq;

// ReSharper disable InconsistentNaming

namespace Lokad.Cqrs.Synthetic
{
    public abstract class Given_Tx_Scenarios
    {
        [DataContract]
        public sealed class Act 
        {
            [DataMember]
            public bool Fail { get; set; }
        }

        public sealed class Setup
        {
            public NuclearStorage Storage;
            public SimpleMessageSender Sender;
            public IPartitionInbox Inbox;
        }

        protected static void Consume(Act message, NuclearStorage storage)
        {
            new TransactionTester
                {
                    OnCommit = () =>
                        {
                            var singleton = storage.AddOrUpdateSingleton(() => 1, i => i + 1);
                            Trace.WriteLine("Commit kicked " + singleton);
                        }
                };

            if (message.Fail)
                throw new InvalidOperationException("Fail requested");
        }

     
        public int TestSpeed = 5000;


        [Test]
        public void Then_transactional_support_is_provided()
        {
            var streamer = EnvelopeStreamer.CreateDefault(typeof(Act));
            var builder = new CqrsEngineBuilder(streamer);
            var setup = ComposeComponents(streamer);

            var handler = new RedirectToCommand();
            handler.WireToLambda<Act>(act => Consume(act, setup.Storage));
            builder.Handle(setup.Inbox, envelope =>
                {
                    using (var tx = new TransactionScope(TransactionScopeOption.RequiresNew))
                    {
                        handler.InvokeMany(envelope.Items.Select(i => i.Content));
                        tx.Complete();
                    }
                });

            using (var source = new CancellationTokenSource())
            using (TestObserver.When<EnvelopeDispatched>(e => source.Cancel()))
            using (var engine = builder.Build())
            {
                setup.Sender.SendBatch(new[] { new Act(), new Act(), new Act { Fail = true } });
                setup.Sender.SendBatch(new[] { new Act(), new Act(), new Act() });


                var task = engine.Start(source.Token);
                //    Trace.WriteLine("Started");
                if (!task.Wait(Debugger.IsAttached ? int.MaxValue : TestSpeed))
                {
                    source.Cancel();
                    Assert.Fail("System should be stopped by now");
                }

                var storage = setup.Storage;
                var count = storage.GetSingletonOrNew<int>();
                Assert.AreEqual(3, count, "Three acts are expected");
            }
        }

        protected abstract Setup ComposeComponents(IEnvelopeStreamer streamer);
    }
}