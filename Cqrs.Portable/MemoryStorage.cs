#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Concurrent;
using System.Linq;
using Lokad.Cqrs.AtomicStorage;
using Lokad.Cqrs.Partition;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Lokad.Cqrs
{
    public static class MemoryStorage
    {
        public static MemoryStorageConfig CreateConfig()
        {
            return new MemoryStorageConfig();
        }

        /// <summary>
        /// Creates the simplified nuclear storage wrapper around Atomic storage.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="strategy">The atomic storage strategy.</param>
        /// <returns></returns>
        public static NuclearStorage CreateNuclear(this MemoryStorageConfig dictionary, IDocumentStrategy strategy)
        {
            var container = new MemoryDocumentStore(dictionary.Data, strategy);
            return new NuclearStorage(container);
        }

        public static MemoryQueueWriterFactory CreateWriteQueueFactory(this MemoryStorageConfig storageConfig)
        {
            return new MemoryQueueWriterFactory(storageConfig);
        }


        public static MemoryPartitionInbox CreateInbox(this MemoryStorageConfig storageConfig,
            params string[] queueNames)
        {
            var queues = queueNames
                .Select(n => storageConfig.Queues.GetOrAdd(n, s => new BlockingCollection<byte[]>()))
                .ToArray();

            return new MemoryPartitionInbox(queues, queueNames);
        }

        public static IQueueWriter CreateQueueWriter(this MemoryStorageConfig storageConfig, string queueName)
        {
            var collection = storageConfig.Queues.GetOrAdd(queueName, s => new BlockingCollection<byte[]>());
            return new MemoryQueueWriter(collection, queueName);
        }

        public static SimpleMessageSender CreateSimpleSender(this MemoryStorageConfig storageConfig,
            IEnvelopeStreamer streamer, string queueName, Func<string> idGenerator = null)
        {
            var queueWriter = new[] {CreateQueueWriter(storageConfig, queueName)};
            return new SimpleMessageSender(streamer, queueWriter, idGenerator);
        }
    }
}