#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.TapeStorage
{
    public class BlobTapeStorageFactory : ITapeContainer
    {
        readonly CloudBlobClient _cloudBlobClient;
        readonly string _containerName;

        readonly ConcurrentDictionary<string, ITapeStream> _writers =
            new ConcurrentDictionary<string, ITapeStream>();

        public BlobTapeStorageFactory(IAzureStorageConfig config, string containerName)
        {
            if (containerName.Any(Char.IsUpper))
                throw new ArgumentException("All letters in a container name must be lowercase.");

            _cloudBlobClient = config.CreateBlobClient();
            _containerName = containerName;
        }

        public ITapeStream GetOrCreateStream(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (string.IsNullOrWhiteSpace("name"))
                throw new ArgumentException("Incorrect value.", "name");

            return _writers.GetOrAdd(
                name,
                s =>
                    {
                        var container = _cloudBlobClient.GetContainerReference(_containerName);
                        return new BlobTapeStream(container, name);
                    });
        }

        public void InitializeForWriting()
        {
            var container = _cloudBlobClient.GetContainerReference(_containerName);
            container.CreateIfNotExist();
        }
    }
}
