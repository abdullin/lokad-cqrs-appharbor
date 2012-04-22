#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.IO;
using System.IO.Compression;
using Lokad.Cqrs.StreamingStorage;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Feature.StreamingStorage
{
    /// <summary>
    /// Azure BLOB implementation of the <see cref="IStreamItem"/>
    /// </summary>
    public sealed class BlobStreamingItem : IStreamItem
    {
        readonly CloudBlob _blob;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobStreamingItem"/> class.
        /// </summary>
        /// <param name="blob">The BLOB.</param>
        public BlobStreamingItem(CloudBlob blob)
        {
            _blob = blob;
        }

        //const string ContentCompression = "gzip";

        public Stream OpenRead()
        {
            return new GZipStream(_blob.OpenRead(), CompressionMode.Decompress, false);
        }

        public Stream OpenWrite()
        {
            return new GZipStream(_blob.OpenWrite(), CompressionMode.Compress, false);
        }

        public bool Exists()
        {
            try
            {
                _blob.FetchAttributes();
                return true;
            }
            catch(StorageClientException ex)
            {
                return false;
            }
            
        }

        /// <summary>
        /// Removes the item, ensuring that the specified condition is met.
        /// </summary>
        public void Delete()
        {
            try
            {
                _blob.Delete();
            }
            catch (StorageClientException ex)
            {
                switch (ex.ErrorCode)
                {
                    case StorageErrorCode.ContainerNotFound:
                        throw StreamErrors.ContainerNotFound(this, ex);
                    case StorageErrorCode.BlobNotFound:
                    case StorageErrorCode.ConditionFailed:
                        return;
                    default:
                        throw;
                }
            }
        }

        /// <summary>
        /// Gets the full path of the current item.
        /// </summary>
        /// <value>The full path.</value>
        public string FullPath
        {
            get { return _blob.Uri.ToString(); }
        }

        /// <summary>
        /// Gets the BLOB reference behind this instance.
        /// </summary>
        /// <value>The reference.</value>
        public CloudBlob Reference
        {
            get { return _blob; }
        }
     }
}