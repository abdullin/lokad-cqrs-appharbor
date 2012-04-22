#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using System;
using System.IO;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.AtomicStorage
{
    /// <summary>
    /// Azure implementation of the view reader/writer
    /// </summary>
    /// <typeparam name="TEntity">The type of the view.</typeparam>
    /// <typeparam name="TKey">the type of the key</typeparam>
    public sealed class AzureAtomicWriter<TKey, TEntity> :
        IDocumentWriter<TKey, TEntity>
        //where TEntity : IAtomicEntity<TKey>
    {
        readonly CloudBlobDirectory _container;
        readonly IDocumentStrategy _strategy;

        public AzureAtomicWriter(CloudBlobClient directory, IDocumentStrategy strategy)
        {
            _strategy = strategy;
            var folderForEntity = strategy.GetEntityBucket<TEntity>();
            _container = directory.GetBlobDirectoryReference(folderForEntity);
        }


        public void InitializeIfNeeded()
        {
            _container.Container.CreateIfNotExist();
        }

        public TEntity AddOrUpdate(TKey key, Func<TEntity> addViewFactory, Func<TEntity, TEntity> updateViewFactory,
            AddOrUpdateHint hint)
        {
            string etag = null;
            var blob = GetBlobReference(key);
            TEntity view;
            try
            {
                // atomic entities should be small, so we can use the simple method
                var bytes = blob.DownloadByteArray();
                using (var stream = new MemoryStream(bytes))
                {
                    view = _strategy.Deserialize<TEntity>(stream);
                }

                view = updateViewFactory(view);
                etag = blob.Attributes.Properties.ETag;
            }
            catch (StorageClientException ex)
            {
                switch (ex.ErrorCode)
                {
                    case StorageErrorCode.ContainerNotFound:
                        var s = string.Format(
                            "Container '{0}' does not exist. You need to initialize this atomic storage and ensure that '{1}' is known to '{2}'.",
                            blob.Container.Name, typeof(TEntity).Name, _strategy.GetType().Name);
                        throw new InvalidOperationException(s, ex);
                    case StorageErrorCode.BlobNotFound:
                    case StorageErrorCode.ResourceNotFound:
                        view = addViewFactory();
                        break;
                    default:
                        throw;
                }
            }
            // atomic entities should be small, so we can use the simple method
            // http://toolheaven.net/post/Azure-and-blob-write-performance.aspx
            using (var memory = new MemoryStream())
            {
                _strategy.Serialize(view, memory);
                // note that upload from stream does weird things
                var bro = etag != null
                    ? new BlobRequestOptions {AccessCondition = AccessCondition.IfMatch(etag)}
                    : new BlobRequestOptions {AccessCondition = AccessCondition.IfNoneMatch("*")};


                // make sure that upload is not rejected due to cashed content MD5
                // http://social.msdn.microsoft.com/Forums/hu-HU/windowsazuredata/thread/4764e38f-b200-4efe-ada2-7de442dc4452
                blob.Properties.ContentMD5 = null;
                blob.UploadByteArray(memory.ToArray(), bro);
            }
            return view;
        }


        public bool TryDelete(TKey key)
        {
            var blob = GetBlobReference(key);
            return blob.DeleteIfExists();
        }

        CloudBlob GetBlobReference(TKey key)
        {
            return _container.GetBlobReference(_strategy.GetEntityLocation(typeof(TEntity), key));
        }
    }
}