#region Copyright (c) 2012 LOKAD SAS. All rights reserved

// You must not remove this notice, or any other, from this software.
// This document is the property of LOKAD SAS and must not be disclosed

#endregion

using System.IO;
using Microsoft.WindowsAzure.StorageClient;
using SaaS.AtomicStorage;

namespace Lokad.Cqrs.AtomicStorage
{
    /// <summary>
    /// Azure implementation of the view reader/writer
    /// </summary>
    /// <typeparam name="TEntity">The type of the view.</typeparam>
    public sealed class AzureAtomicReader<TKey, TEntity> :
        IDocumentReader<TKey, TEntity>
    {
        readonly CloudBlobDirectory _container;
        readonly IDocumentStrategy _strategy;

        public AzureAtomicReader(CloudBlobClient storage, IDocumentStrategy strategy)
        {
            _strategy = strategy;
            var folder = strategy.GetEntityBucket<TEntity>();
            _container = storage.GetBlobDirectoryReference(folder);
        }

        CloudBlob GetBlobReference(TKey key)
        {
            return _container.GetBlobReference(_strategy.GetEntityLocation(typeof(TEntity), key));
        }

        public bool TryGet(TKey key, out TEntity entity)
        {
            var blob = GetBlobReference(key);
            try
            {
                // blob request options are cloned from the config
                // atomic entities should be small, so we can use the simple method
                var bytes = blob.DownloadByteArray();
                using (var stream = new MemoryStream(bytes))
                {
                    entity = _strategy.Deserialize<TEntity>(stream);
                    return true;
                }
            }
            catch (StorageClientException ex)
            {
                switch (ex.ErrorCode)
                {
                    case StorageErrorCode.ContainerNotFound:
                    case StorageErrorCode.BlobNotFound:
                    case StorageErrorCode.ResourceNotFound:
                        entity = default(TEntity);
                        return false;
                    default:
                        throw;
                }
            }
        }
    }
}