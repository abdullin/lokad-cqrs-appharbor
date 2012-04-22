#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;

namespace Lokad.Cqrs.AtomicStorage
{
    /// <summary>
    /// Basic usability wrapper for the atomic storage operations, that does not enforce concurrency handling. 
    /// If you want to work with advanced functionality, either request specific interfaces from the container 
    /// or go through the advanced members on this instance. 
    /// </summary>
    public sealed class NuclearStorage : HideObjectMembersFromIntelliSense
    {
        public readonly IDocumentStore Container;


        public NuclearStorage(IDocumentStore container)
        {
            Container = container;
        }

        public void CopyFrom(NuclearStorage source, params string[] buckets)
        {
            foreach (var bucket in buckets)
            {
                Container.WriteContents(bucket, source.Container.EnumerateContents(bucket));
            }
        }

        public bool TryDeleteEntity<TEntity>(object key)
        {
            return Container.GetWriter<object, TEntity>().TryDelete(key);
        }

        public bool TryDeleteSingleton<TEntity>()
        {
            return Container.GetWriter<unit, TEntity>().TryDelete(unit.it);
        }

        public TEntity UpdateEntity<TEntity>(object key, Action<TEntity> update)
        {
            return Container.GetWriter<object, TEntity>().UpdateOrThrow(key, update);
        }


        public TSingleton UpdateSingletonOrThrow<TSingleton>(Action<TSingleton> update)
        {
            return Container.GetWriter<unit, TSingleton>().UpdateOrThrow(unit.it, update);
        }


        public Optional<TEntity> GetEntity<TEntity>(object key)
        {
            return Container.GetReader<object, TEntity>().Get(key);
        }

        public bool TryGetEntity<TEntity>(object key, out TEntity entity)
        {
            return Container.GetReader<object, TEntity>().TryGet(key, out entity);
        }

        public TEntity AddOrUpdateEntity<TEntity>(object key, TEntity entity)
        {
            return Container.GetWriter<object, TEntity>().AddOrUpdate(key, () => entity, source => entity);
        }

        public TEntity AddOrUpdateEntity<TEntity>(object key, Func<TEntity> addFactory, Action<TEntity> update)
        {
            return Container.GetWriter<object, TEntity>().AddOrUpdate(key, addFactory, update);
        }

        public TEntity AddOrUpdateEntity<TEntity>(object key, Func<TEntity> addFactory, Func<TEntity, TEntity> update)
        {
            return Container.GetWriter<object, TEntity>().AddOrUpdate(key, addFactory, update);
        }

        public TEntity AddEntity<TEntity>(object key, TEntity newEntity)
        {
            return Container.GetWriter<object, TEntity>().Add(key, newEntity);
        }

        public TSingleton AddOrUpdateSingleton<TSingleton>(Func<TSingleton> addFactory, Action<TSingleton> update)
        {
            return Container.GetWriter<unit, TSingleton>().AddOrUpdate(unit.it, addFactory, update);
        }

        public TSingleton AddOrUpdateSingleton<TSingleton>(Func<TSingleton> addFactory,
            Func<TSingleton, TSingleton> update)
        {
            return Container.GetWriter<unit, TSingleton>().AddOrUpdate(unit.it, addFactory, update);
        }

        public TSingleton UpdateSingletonEnforcingNew<TSingleton>(Action<TSingleton> update) where TSingleton : new()
        {
            return Container.GetWriter<unit, TSingleton>().UpdateEnforcingNew(unit.it, update);
        }

        public TSingleton GetSingletonOrNew<TSingleton>() where TSingleton : new()
        {
            return Container.GetReader<unit, TSingleton>().GetOrNew();
        }

        public Optional<TSingleton> GetSingleton<TSingleton>()
        {
            return Container.GetReader<unit, TSingleton>().Get();
        }
    }
}