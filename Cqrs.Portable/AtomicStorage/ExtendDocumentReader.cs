#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;

namespace SaaS.AtomicStorage
{
    public static class ExtendDocumentReader
    {
        public static Optional<TEntity> Get<TKey, TEntity>(this IDocumentReader<TKey, TEntity> self, TKey key)
        {
            TEntity entity;
            if (self.TryGet(key, out entity))
            {
                return entity;
            }
            return Optional<TEntity>.Empty;
        }

        public static TEntity Load<TKey, TEntity>(this IDocumentReader<TKey, TEntity> self, TKey key)
        {
            TEntity entity;
            if (self.TryGet(key, out entity))
            {
                return entity;
            }
            var txt = string.Format("Failed to load '{0}' with key '{1}'.", typeof(TEntity).Name, key);
            throw new InvalidOperationException(txt);
        }

        public static TView GetOrNew<TView>(this IDocumentReader<unit, TView> reader)
            where TView : new()
        {
            TView view;
            if (reader.TryGet(unit.it, out view))
            {
                return view;
            }
            return new TView();
        }

        public static Optional<TSingleton> Get<TSingleton>(this IDocumentReader<unit, TSingleton> reader)
        {
            TSingleton singleton;
            if (reader.TryGet(unit.it, out singleton))
            {
                return singleton;
            }
            return Optional<TSingleton>.Empty;
        }
    }
}