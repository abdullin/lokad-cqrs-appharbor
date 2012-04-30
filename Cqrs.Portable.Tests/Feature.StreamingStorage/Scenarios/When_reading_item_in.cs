#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Threading;
using Lokad.Cqrs.StreamingStorage;
using NUnit.Framework;

// ReSharper disable InconsistentNaming

namespace Lokad.Cqrs.Feature.StreamingStorage.Scenarios
{
    public abstract class When_reading_item_in<TStorage> :
        StorageItemFixture<TStorage> where TStorage : ITestStorage, new()
    {
        [Test, Ignore("Azure dev fabric messes up the compliance")]
        public void Missing_container_throws_container_not_found()
        {
            ExpectContainerNotFound(() => TryToRead(TestItem));
        }

        [Test]
        public void Missing_item_throws_item_not_found()
        {
            TestContainer.Create();

            ExpectItemNotFound(() => TryToRead(TestItem));
        }

       



 

        [Test]
        public void Valid_item_returns()
        {
            TestContainer.Create();
            var g = Guid.NewGuid();

            Write(TestItem, g);
            ShouldHaveGuid(TestItem, g);
        }
    }
}