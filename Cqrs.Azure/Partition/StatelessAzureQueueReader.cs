#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Threading;
using Lokad.Cqrs.Dispatch.Events;
using Lokad.Cqrs.Envelope;
using Lokad.Cqrs.Partition;
using Lokad.Cqrs.Partition.Events;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Feature.AzurePartition
{
    public sealed class StatelessAzureQueueReader
    {
        readonly TimeSpan _visibilityTimeout;

        readonly CloudBlobDirectory _cloudBlob;
        readonly Lazy<CloudQueue> _posionQueue;
        readonly CloudQueue _queue;
        readonly string _queueName;

        public string Name
        {
            get { return _queueName; }
        }


        public StatelessAzureQueueReader(
            string name,
            CloudQueue primaryQueue,
            CloudBlobDirectory container,
            Lazy<CloudQueue> poisonQueue,
            TimeSpan visibilityTimeout)
        {
            _cloudBlob = container;
            _queue = primaryQueue;
            _posionQueue = poisonQueue;
            _queueName = name;
            _visibilityTimeout = visibilityTimeout;
        }

        bool _initialized;

        public void InitIfNeeded()
        {
            if (_initialized)
                return;
            _queue.CreateIfNotExist();
            _cloudBlob.Container.CreateIfNotExist();
            _initialized = true;
        }

        public GetEnvelopeResult TryGetMessage()
        {
            CloudQueueMessage message;
            try
            {
                message = _queue.GetMessage(_visibilityTimeout);
            }
            catch (ThreadAbortException)
            {
                // we are stopping
                return GetEnvelopeResult.Empty;
            }
            catch (Exception ex)
            {
                SystemObserver.Notify(new FailedToReadMessage(ex, _queueName));
                return GetEnvelopeResult.Error();
            }

            if (null == message)
            {
                return GetEnvelopeResult.Empty;
            }
            
            try
            {
                var unpacked = DownloadPackage(message);
                return GetEnvelopeResult.Success(unpacked);
            }
            catch (StorageClientException ex)
            {
                SystemObserver.Notify(new FailedToAccessStorage(ex, _queue.Name, message.Id));
                return GetEnvelopeResult.Retry;
            }
            catch (Exception ex)
            {
                SystemObserver.Notify(new MessageInboxFailed(ex, _queue.Name, message.Id));
                // new poison details
                _posionQueue.Value.AddMessage(message);
                _queue.DeleteMessage(message);
                return GetEnvelopeResult.Retry;
            }
        }

        MessageTransportContext DownloadPackage(CloudQueueMessage message)
        {
            var buffer = message.AsBytes;

            EnvelopeReference reference;
            if (AzureMessageOverflows.TryReadAsEnvelopeReference(buffer, out reference))
            {
                if (reference.StorageContainer != _cloudBlob.Uri.ToString())
                    throw new InvalidOperationException("Wrong container used!");
                var blob = _cloudBlob.GetBlobReference(reference.StorageReference);
                buffer = blob.DownloadByteArray();
            }
            return new MessageTransportContext(message, buffer, _queueName);
        }


        /// <summary>
        /// ACKs the message by deleting it from the queue.
        /// </summary>
        /// <param name="message">The message context to ACK.</param>
        public void AckMessage(MessageTransportContext message)
        {
            if (message == null) throw new ArgumentNullException("message");
            _queue.DeleteMessage((CloudQueueMessage) message.TransportMessage);
        }
    }
}