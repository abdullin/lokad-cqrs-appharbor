#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;

namespace SaaS.AtomicStorage
{
    /// <summary>
    /// View writer interface, used by the event handlers
    /// </summary>
    /// <typeparam name="TEntity">The type of the view.</typeparam>
    /// <typeparam name="TKey">type of the key</typeparam>
    public interface IDocumentWriter<in TKey, TEntity> //where TEntity : IAtomicEntity<TKey>
    {
        TEntity AddOrUpdate(TKey key, Func<TEntity> addFactory, Func<TEntity, TEntity> update,
            AddOrUpdateHint hint = AddOrUpdateHint.ProbablyExists);

        bool TryDelete(TKey key);
    }
}