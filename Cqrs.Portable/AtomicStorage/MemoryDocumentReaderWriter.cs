#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Concurrent;
using System.IO;

namespace Lokad.Cqrs.AtomicStorage
{
    public sealed class MemoryDocumentReaderWriter<TKey, TEntity> : IDocumentReader<TKey, TEntity>,
                                                                    IDocumentWriter<TKey, TEntity>
    {
        readonly IDocumentStrategy _strategy;
        readonly ConcurrentDictionary<string, byte[]> _store;

        public MemoryDocumentReaderWriter(IDocumentStrategy strategy, ConcurrentDictionary<string, byte[]> store)
        {
            _store = store;
            _strategy = strategy;
        }

        string GetName(TKey key)
        {
            return _strategy.GetEntityLocation(typeof(TEntity), key);
        }

        public bool TryGet(TKey key, out TEntity entity)
        {
            var name = GetName(key);
            byte[] bytes;
            if (_store.TryGetValue(name, out bytes))
            {
                using (var mem = new MemoryStream(bytes))
                {
                    entity = _strategy.Deserialize<TEntity>(mem);
                    return true;
                }
            }
            entity = default(TEntity);
            return false;
        }


        public TEntity AddOrUpdate(TKey key, Func<TEntity> addFactory, Func<TEntity, TEntity> update,
            AddOrUpdateHint hint)
        {
            var result = default(TEntity);
            _store.AddOrUpdate(GetName(key), s =>
                {
                    result = addFactory();
                    using (var memory = new MemoryStream())
                    {
                        _strategy.Serialize(result, memory);
                        return memory.ToArray();
                    }
                }, (s2, bytes) =>
                    {
                        TEntity entity;
                        using (var memory = new MemoryStream(bytes))
                        {
                            entity = _strategy.Deserialize<TEntity>(memory);
                        }
                        result = update(entity);
                        using (var memory = new MemoryStream())
                        {
                            _strategy.Serialize(result, memory);
                            return memory.ToArray();
                        }
                    });
            return result;
        }


        public bool TryDelete(TKey key)
        {
            byte[] bytes;
            return _store.TryRemove(GetName(key), out bytes);
        }
    }
}