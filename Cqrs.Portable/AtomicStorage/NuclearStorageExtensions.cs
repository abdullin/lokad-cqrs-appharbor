#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;

namespace Lokad.Cqrs.AtomicStorage
{
    public static class NuclearStorageExtensions
    {
        public static TSingleton UpdateSingletonEnforcingNew<TSingleton>(this NuclearStorage storage,
            Action<TSingleton> update)
            where TSingleton : new()
        {
            return storage.Container.GetWriter<unit, TSingleton>().UpdateEnforcingNew(unit.it, update);
        }

        public static TEntity UpdateEntityEnforcingNew<TEntity>(this NuclearStorage storage, object key,
            Action<TEntity> update)
            where TEntity : new()
        {
            return storage.Container.GetWriter<object, TEntity>().UpdateEnforcingNew(key, update);
        }
    }
}