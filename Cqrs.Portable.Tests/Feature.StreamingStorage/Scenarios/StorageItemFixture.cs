#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.IO;
using Lokad.Cqrs.StreamingStorage;
using NUnit.Framework;

namespace Lokad.Cqrs.Feature.StreamingStorage.Scenarios
{
    public abstract class StorageItemFixture<TStorage>
        where TStorage : ITestStorage, new()
    {
        readonly ITestStorage _factory = new TStorage();


        static void Expect<TEx>(Action action) where TEx : StreamBaseException
        {
            try
            {
                action();
                Assert.Fail("Expected exception '{0}', but got nothing", typeof (TEx));
            }
            catch (TEx)
            {
            }
        }


        IStreamContainer GetContainer(string path)
        {
            return _factory.GetContainer(path);
        }

        protected IStreamContainer TestContainer { get; private set; }
        protected IStreamItem TestItem { get; private set; }

        [SetUp]
        public void SetUp()
        {
            TestContainer = GetContainer("tc-" + Guid.NewGuid().ToString().ToLowerInvariant());
            TestItem = TestContainer.GetItem(Guid.NewGuid().ToString().ToLowerInvariant());
        }

        [TearDown]
        public void TearDown()
        {
            TestContainer.Delete();
        }

        protected IStreamItem GetItem(string path)
        {
            return TestContainer.GetItem(path);
        }


        protected void ExpectContainerNotFound(Action action)
        {
            Expect<StreamContainerNotFoundException>(action);
        }

        protected void ExpectItemNotFound(Action action)
        {
            Expect<StreamItemNotFoundException>(action);
        }
        protected void Write(IStreamItem streamingItem, Guid g)
        {
            streamingItem.Write(stream => stream.Write(g.ToByteArray(), 0, 16));
        }

        protected void Write(IStreamItem streamingItem, byte[] bytes)
        {
            streamingItem.Write(stream => stream.Write(bytes, 0, bytes.Length));
        }

        protected void TryToRead(IStreamItem item)
        {
            item.ReadInto((stream) => stream.Read(new byte[1], 0, 1));
        }

        protected void ShouldHaveGuid(IStreamItem streamingItem, Guid g)
        {
            var set = false;
            Guid actual = Guid.Empty;
            
            streamingItem.ReadInto((stream) =>
                {
                    var b = new byte[16];
                    stream.Read(b, 0, 16);
                    actual = new Guid(b);
                    set = true;
                });

            Assert.AreEqual(g, actual);
            
            
            set = true;

            Assert.IsTrue(set);
        }

        protected void ShouldHaveBytes(IStreamItem streamingItem, byte[] bytes)
        {
            byte[] actualBytes = null;

            using (var ms = new MemoryStream())
            {
                streamingItem.ReadInto((stream) =>
                    {
                        stream.CopyTo(ms);
                        actualBytes = ms.ToArray();
                    });

            }
            Assert.AreEqual(bytes, actualBytes);
        }
    }
}