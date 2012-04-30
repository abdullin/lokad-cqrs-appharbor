#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading;
using Lokad.Cqrs.Build;
using Lokad.Cqrs.Core.Envelope;
using Lokad.Cqrs.Envelope;
using Lokad.Cqrs.Envelope.Events;
using NUnit.Framework;

namespace Lokad.Cqrs.Synthetic
{
    [TestFixture]
    public sealed class Given_duplicate_configuration
    {
        // ReSharper disable InconsistentNaming

        [DataContract]
        public sealed class Message {}

        [Test]
        public void Dulicate_message_is_detected()
        {
            var streamer = EnvelopeStreamer.CreateDefault(typeof(Message));

            var builder = new CqrsEngineBuilder(streamer);

            var cfg = new MemoryStorageConfig();

            var sender = cfg.CreateSimpleSender(streamer, "in");
            builder.Handle(cfg.CreateInbox("in"), envelope => Console.WriteLine("Got message"));

            var env = new EnvelopeBuilder("fixed ID").Build();

            using (var token = new CancellationTokenSource())
            using (var build = builder.Build())
            using (TestObserver.When<EnvelopeDuplicateDiscarded>(discarded => token.Cancel()))
            {
                sender.SendBatch(new object[]{new Message()}, IdGeneration.HashContent);
                sender.SendBatch(new object[] { new Message()}, IdGeneration.HashContent);
                build.Start(token.Token);

                if (Debugger.IsAttached)
                {
                    token.Token.WaitHandle.WaitOne();
                }
                else
                {
                    token.Token.WaitHandle.WaitOne(10000);
                }

                Assert.IsTrue(token.IsCancellationRequested);
            }
        }
    }
}