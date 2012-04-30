#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.IO;

namespace SaaS.Partition
{
    public sealed class FileQueueWriterFactory : IQueueWriterFactory
    {
        readonly FileStorageConfig _account;
        readonly string _endpoint;

        public FileQueueWriterFactory(FileStorageConfig account)
        {
            _account = account;
            _endpoint = _account.AccountName;
        }

        public string Endpoint
        {
            get { return _endpoint; }
        }

        public IQueueWriter GetWriteQueue(string queueName)
        {
            var full = Path.Combine(_account.Folder.FullName, queueName);
            if (!Directory.Exists(full))
            {
                Directory.CreateDirectory(full);
            }
            return
                new FileQueueWriter(new DirectoryInfo(full), queueName);
        }
    }
}