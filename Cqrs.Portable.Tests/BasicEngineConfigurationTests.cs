#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using System;
using System.Runtime.Serialization;
using System.Threading;
using Lokad.Cqrs.Build;
using Lokad.Cqrs.Core;
using Lokad.Cqrs.Dispatch.Events;
using Lokad.Cqrs.Envelope;
using Lokad.Cqrs.Partition;
using NUnit.Framework;

namespace Lokad.Cqrs
{
    [TestFixture]
    public sealed class BasicEngineConfigurationTests
    {
        // ReSharper disable InconsistentNaming
        static void TestConfiguration(IQueueWriter sender, CqrsEngineBuilder builder)
        {
            int i = 0;
            using (var t = new CancellationTokenSource())
            using (TestObserver.When<MessageAcked>(ea =>
                {
                    if (ea.Context.QueueName != "do")
                        return;
                    if (i++ >= 5)
                        t.Cancel();
                }))
            using (var engine = builder.Build())
            {
                engine.Start(t.Token);
                sender.PutMessage(new byte[1]);
                if (!t.Token.WaitHandle.WaitOne(5000))
                {
                    t.Cancel();
                }
                Assert.IsTrue(t.IsCancellationRequested);
            }
        }

        

        [Test]
        public void PartitionWithRouter()
        {
            var config = new MemoryStorageConfig();
            var raw = new CqrsEngineBuilder(EnvelopeStreamer.CreateDefault());

            var inWriter = config.CreateQueueWriter("in");
            var doWriter = config.CreateQueueWriter("do");
            
            // forwarder in => do 
            raw.Dispatch(config.CreateInbox("in"), doWriter.PutMessage);
            // forwarder do => in
            raw.Dispatch(config.CreateInbox("do"), inWriter.PutMessage);


            TestConfiguration(inWriter, raw);
        }


        [Test]
        public void Direct()
        {
            var config = new MemoryStorageConfig();
            var raw = new CqrsEngineBuilder(EnvelopeStreamer.CreateDefault());
            var doWriter = config.CreateQueueWriter("do");

            // forwarder do => do
            raw.Dispatch(config.CreateInbox("do"), doWriter.PutMessage);
            TestConfiguration(doWriter, raw);
        }

        static void LoadBalance(byte[] message, params IQueueWriter[] writers)
        {
            var rand = new Random();
            int index = rand.Next(writers.Length);
            writers[index].PutMessage(message);

        }

        [Test]
        public void RouterChain()
        {
            var config = new MemoryStorageConfig();
            var raw = new CqrsEngineBuilder(EnvelopeStreamer.CreateDefault());
            var doWriter = config.CreateQueueWriter("do");

            var route1 = config.CreateQueueWriter("route1");
            var route2 = config.CreateQueueWriter("route2");

            // in => (route1 OR route2)
            raw.Dispatch(config.CreateInbox("in"), bytes => LoadBalance(bytes, route1, route2));
            // forwarder (route1,route2) => do
            raw.Dispatch(config.CreateInbox("route1", "route2"), doWriter.PutMessage);

            raw.Dispatch(config.CreateInbox("do"), bytes => LoadBalance(bytes, route1, route2));
            TestConfiguration(config.CreateQueueWriter("in"), raw);
        }
    }
}