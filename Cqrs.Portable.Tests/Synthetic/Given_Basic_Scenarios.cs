#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using System;
using System.Runtime.Serialization;
using System.Threading;
using Lokad.Cqrs.AtomicStorage;
using Lokad.Cqrs.Build;
using Lokad.Cqrs.Core.Envelope;
using Lokad.Cqrs.Dispatch.Events;
using Lokad.Cqrs.Envelope;
using Lokad.Cqrs.Envelope.Events;
using Lokad.Cqrs.Feature.AtomicStorage;
using Lokad.Cqrs.Partition;
using NUnit.Framework;

// ReSharper disable InconsistentNaming

namespace Lokad.Cqrs.Synthetic
{
    public abstract class Given_Basic_Scenarios
    {
        [DataContract]
        public sealed class FailingMessage
        {
            [DataMember]
            public int FailXTimes { get; set; }
        }

        public sealed class Setup
        {
            public NuclearStorage Store;
            public SimpleMessageSender Sender;
            public IPartitionInbox Inbox;
        }

        readonly IEnvelopeStreamer _streamer = EnvelopeStreamer.CreateDefault(typeof(FailingMessage));

        CqrsEngineBuilder BootstrapHandlers(Setup setup)
        {
            var builder = new CqrsEngineBuilder(_streamer);
            var handler = new RedirectToCommand();
            handler.WireToLambda<FailingMessage>(am => SmartFailing(am, setup.Store));
            builder.Handle(setup.Inbox, envelope =>
                {
                    foreach (var message in envelope.Items)
                    {
                        handler.Invoke(message.Content);
                    }
                });
            return builder;
        }


        protected static void SmartFailing(FailingMessage message, NuclearStorage storage)
        {
            var status = storage.GetSingletonOrNew<int>();
            if (status < message.FailXTimes)
            {
                storage.AddOrUpdateSingleton(() => 1, i => i + 1);
                throw new InvalidOperationException("Failure requested");
            }
        }


        protected abstract Setup ConfigureComponents(IEnvelopeStreamer config);

        protected int TestSpeed = 2000;

        [Test]
        public void Then_permanent_failure_is_quarantined()
        {
            var setup = ConfigureComponents(_streamer);
            var builder = BootstrapHandlers(setup);

            using (var source = new CancellationTokenSource())
            using (TestObserver.When<EnvelopeQuarantined>(e => source.Cancel()))
            using (var engine = builder.Build())
            {
                setup.Sender.SendOne(new FailingMessage
                    {
                        FailXTimes = 50
                    });
                var task = engine.Start(source.Token);

                if (!task.Wait(TestSpeed))
                {
                    source.Cancel();
                    Assert.Fail("System should be stopped by now");
                }
            }
        }

        [Test]
        public void Then_transient_failure_works()
        {
            var setup = ConfigureComponents(_streamer);
            var builder = BootstrapHandlers(setup);

            using (var source = new CancellationTokenSource())
            using (TestObserver.When<MessageAcked>(e => source.Cancel()))
            using (var engine = builder.Build())
            {
                setup.Sender.SendOne(new FailingMessage
                    {
                        FailXTimes = 1
                    });
                var task = engine.Start(source.Token);
                Console.WriteLine(task.Status);
                if (!task.Wait(TestSpeed))
                {
                    source.Cancel();
                    Assert.Fail("System should be stopped by now");
                }
            }
        }
    }
}