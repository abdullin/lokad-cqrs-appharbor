#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using System.Runtime.Serialization;
using System.Threading;
using Lokad.Cqrs.Build;
using NUnit.Framework;

namespace Lokad.Cqrs
{
    [TestFixture]
    public sealed class BasicClientConfigurationTests
    {
        // ReSharper disable InconsistentNaming
        [Test]
        public void Test()
        {
            using (var source = new CancellationTokenSource())
            {
                var dev = AzureStorage.CreateConfigurationForDev();
                WipeAzureAccount.Fast(s => s.StartsWith("test-"), dev);
                var b = new CqrsEngineBuilder(null);
                b.Dispatch(dev.CreateInbox("test-publish"), bytes =>
                    {
                        if (bytes[0] == 42)
                            source.Cancel();
                    });


                using (var engine = b.Build())
                {
                    var task = engine.Start(source.Token);

                    dev.CreateQueueWriter("test-publish").PutMessage(new byte[] {42});
                    if (!task.Wait(5000))
                    {
                        source.Cancel();
                    }
                }
            }
        }
    }
}