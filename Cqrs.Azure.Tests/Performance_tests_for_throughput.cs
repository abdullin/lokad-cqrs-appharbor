#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using System;
using System.Diagnostics;
using System.Threading;
using Lokad.Cqrs.Build;
using Lokad.Cqrs.Dispatch.Events;
using NUnit.Framework;

// ReSharper disable InconsistentNaming

namespace Lokad.Cqrs
{
    [TestFixture, Explicit]
    public sealed class Performance_throughput_tests
    {
        [Test]
        public void Memory_lambda()
        {
            var config = new MemoryStorageConfig();
            var writer = config.CreateQueueWriter("test");
            var inbox = config.CreateInbox("test");

            var builder = new CqrsEngineBuilder(null);
            builder.Dispatch(inbox, bytes => { });

            var setup = new Setup
                {
                    Send = i => writer.PutMessage(new[] {i}),
                    Engine = builder.Build()
                };

            TestConfiguration(setup, 1000000);
        }


        [Test]
        public void Throughput_Azure_lambda()
        {
            var config = AzureStorage.CreateConfigurationForDev();
            WipeAzureAccount.Fast(s => s.StartsWith("throughput"), config);

            var writer = config.CreateQueueWriter("test");
            var inbox = config.CreateInbox("test", u => TimeSpan.Zero);
            var builder = new CqrsEngineBuilder(null);
            builder.Dispatch(inbox, bytes => { });

            var setup = new Setup
                {
                    Send = i => writer.PutMessage(new[] {i}),
                    Engine = builder.Build()
                };

            TestConfiguration(setup, 100);
        }

        [Test]
        public void File_lambda()
        {
            var config = FileStorage.CreateConfig("throughput-tests");
            config.Wipe();

            var writer = config.CreateQueueWriter("test");
            var inbox = config.CreateInbox("test",  u => TimeSpan.FromMilliseconds(0));
            var builder = new CqrsEngineBuilder(null);
            builder.Dispatch(inbox, bytes => { });

            var setup = new Setup
                {
                    Send = i => writer.PutMessage(new[] {i}),
                    Engine = builder.Build()
                };

            TestConfiguration(setup, 1000);
        }

        sealed class Setup : IDisposable
        {
            public readonly CancellationTokenSource Source = new CancellationTokenSource();
            public Action<byte> Send;
            public CqrsEngineHost Engine;

            public void Dispose()
            {
                Source.Dispose();
                Engine.Dispose();
            }
        }

        static void TestConfiguration(Setup setup, int useMessages)
        {
            var step = (useMessages / 5);
            int count = 0;
            var watch = new Stopwatch();


            using (TestObserver.When<MessageAcked>(ea =>
                {
                    count += 1;

                    if ((count % step) == 0)
                    {
                        var messagesPerSecond = count / watch.Elapsed.TotalSeconds;
                        Console.WriteLine("{0} - {1}", count, Math.Round(messagesPerSecond, 1));
                    }
                    if (ea.Context.Unpacked[0] == 42)
                    {
                        setup.Source.Cancel();
                    }
                }, includeTracing:false))
            using (setup.Source)
            using (setup.Engine)
            {
                // first we send X then we check

                for (int i = 0; i < useMessages; i++)
                {
                    setup.Send(1);
                }
                setup.Send(42);
                watch.Start();
                setup.Engine.Start(setup.Source.Token);
                setup.Source.Token.WaitHandle.WaitOne(10000);
            }
        }
    }
}