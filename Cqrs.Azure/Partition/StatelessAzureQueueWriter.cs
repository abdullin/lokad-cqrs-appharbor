#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Lokad.Cqrs.Envelope;
using Lokad.Cqrs.Partition;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Feature.AzurePartition
{
    public sealed class StatelessAzureQueueWriter : IQueueWriter
    {
        public string Name { get; private set; }
        public void PutMessage(byte[] envelope)
        {
            var packed = PrepareCloudMessage(envelope);
            _queue.AddMessage(packed);
        }

        CloudQueueMessage PrepareCloudMessage(byte[] buffer)
        {
            if (buffer.Length < AzureMessageOverflows.CloudQueueLimit)
            {
                // write message to queue
                return new CloudQueueMessage(buffer);
            }
            // ok, we didn't fit, so create reference message
            var referenceId = DateTimeOffset.UtcNow.ToString(DateFormatInBlobName) + "-" + Guid.NewGuid().ToString().ToLowerInvariant();
            _cloudBlob.GetBlobReference(referenceId).UploadByteArray(buffer);
            var reference = new EnvelopeReference(_cloudBlob.Uri.ToString(), referenceId);
            var blob = AzureMessageOverflows.SaveEnvelopeReference(reference);
            return new CloudQueueMessage(blob);
        }

        public StatelessAzureQueueWriter(CloudBlobContainer container, CloudQueue queue, string name)
        {
            _cloudBlob = container;
            _queue = queue;
            Name = name;
        }

        public void Init()
        {
            _queue.CreateIfNotExist();
            _cloudBlob.CreateIfNotExist();
        }


        const string DateFormatInBlobName = "yyyy-MM-dd-HH-mm-ss-ffff";
        readonly CloudBlobContainer _cloudBlob;
        readonly CloudQueue _queue;
    }
}