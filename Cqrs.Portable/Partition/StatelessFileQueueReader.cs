#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using SaaS.Dispatch.Events;
using SaaS.Partition.Events;

namespace SaaS.Partition
{
    public sealed class StatelessFileQueueReader
    {
        readonly DirectoryInfo _queue;
        readonly string _queueName;

        public string Name
        {
            get { return _queueName; }
        }

        public StatelessFileQueueReader(DirectoryInfo queue, string queueName)
        {
            _queue = queue;
            _queueName = queueName;
        }

        public StatelessFileQueueReader(string path, string queueName)
        {
            _queue = new DirectoryInfo(path);
            _queueName = queueName;
        }

        public GetEnvelopeResult TryGetMessage()
        {
            FileInfo message;
            try
            {
                message = _queue.EnumerateFiles().FirstOrDefault();
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
                var buffer = File.ReadAllBytes(message.FullName);

                var unpacked = new MessageTransportContext(message, buffer, _queueName);
                return GetEnvelopeResult.Success(unpacked);
            }
            catch (IOException ex)
            {
                // this is probably sharing violation, no need to 
                // scare people.
                if (!IsSharingViolation(ex))
                {
                    SystemObserver.Notify(new FailedToAccessStorage(ex, _queue.Name, message.Name));
                }
                return GetEnvelopeResult.Retry;
            }
            catch (Exception ex)
            {
                SystemObserver.Notify(new MessageInboxFailed(ex, _queue.Name, message.FullName));
                // new poison details
                return GetEnvelopeResult.Retry;
            }
        }

        static bool IsSharingViolation(IOException ex)
        {
            // http://stackoverflow.com/questions/425956/how-do-i-determine-if-an-ioexception-is-thrown-because-of-a-sharing-violation
            // don't ask...
            var hResult = Marshal.GetHRForException(ex);
            const int sharingViolation = 32;
            return (hResult & 0xFFFF) == sharingViolation;
        }

        public void InitIfNeeded()
        {
            _queue.Create();
        }

        /// <summary>
        /// ACKs the message by deleting it from the queue.
        /// </summary>
        /// <param name="message">The message context to ACK.</param>
        public void AckMessage(MessageTransportContext message)
        {
            if (message == null) throw new ArgumentNullException("message");
            ((FileInfo) message.TransportMessage).Delete();
        }
    }
}