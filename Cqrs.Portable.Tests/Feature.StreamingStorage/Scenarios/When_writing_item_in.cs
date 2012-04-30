#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Lokad.Cqrs.StreamingStorage;
using NUnit.Framework;

// ReSharper disable InconsistentNaming

namespace Lokad.Cqrs.Feature.StreamingStorage.Scenarios
{
    public abstract class When_writing_item_in<TStorage> : StorageItemFixture<TStorage>
        where TStorage : ITestStorage, new()
    {
        [Test]
        public void Missing_container_throws_container_not_found()
        {
            ExpectContainerNotFound(() => Write(TestItem, Guid.Empty));
        }

        

        


        [Test]
        public void Unconditional_append_works()
        {
            TestContainer.Create();
            Write(TestItem, Guid.Empty);
        }

       

        [Test]
        public void Unconditional_upsert_works()
        {
            TestContainer.Create();
            Write(TestItem, Guid.Empty);
            Write(TestItem, Guid.Empty);
        }

        [Test]
        public void Writing_less_shrunk_data()
        {
            TestContainer.Create();
            var bytes = new byte[100];
            Write(TestItem, bytes);
            ShouldHaveBytes(TestItem, bytes);

            bytes = new byte[2];
            Write(TestItem, bytes);
            ShouldHaveBytes(TestItem, bytes);
        }
    }
}