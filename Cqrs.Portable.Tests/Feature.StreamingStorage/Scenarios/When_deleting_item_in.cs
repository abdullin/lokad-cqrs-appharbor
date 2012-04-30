﻿#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Lokad.Cqrs.StreamingStorage;
using NUnit.Framework;

// ReSharper disable InconsistentNaming

namespace Lokad.Cqrs.Feature.StreamingStorage.Scenarios
{
    public abstract class When_deleting_item_in<TStorage> :
        StorageItemFixture<TStorage> where TStorage : ITestStorage, new()
    {
        [Test]
        public void Missing_container_throws_container_not_found()
        {
            ExpectContainerNotFound(() => TestItem.Delete());
        }

        [Test]
        public void Missing_item_works()
        {
            TestContainer.Create();
            TestItem.Delete();
        }

    }
}