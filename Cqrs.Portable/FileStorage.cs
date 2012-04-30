#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.IO;
using SaaS.AtomicStorage;
using SaaS.Evil;
using SaaS.Partition;
using SaaS.StreamingStorage;
using SaaS.TapeStorage;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace SaaS
{
    public static class FileStorage
    {
        public static NuclearStorage CreateNuclear(this FileStorageConfig config, IDocumentStrategy strategy)
        {
            var factory = new FileDocumentStore(config.FullPath, strategy);
            return new NuclearStorage(factory);
        }


        public static IStreamRoot CreateStreaming(this FileStorageConfig config)
        {
            var path = config.FullPath;
            var container = new FileStreamContainer(path);
            container.Create();
            return container;
        }

        public static IStreamContainer CreateStreaming(this FileStorageConfig config, string subfolder)
        {
            return config.CreateStreaming().GetContainer(subfolder).Create();
        }

        public static FileTapeContainer CreateTape(this FileStorageConfig config, string subfolder)
        {
            return CreateTape(config.SubFolder(subfolder));
        }

        public static FileTapeContainer CreateTape(this FileStorageConfig config)
        {
            var factory = new FileTapeContainer(config.FullPath);
            factory.InitializeForWriting();
            return factory;
        }

        public static FileStorageConfig CreateConfig(string fullPath, string optionalName = null, bool reset = false)
        {
            var folder = new DirectoryInfo(fullPath);
            var config = new FileStorageConfig(folder, optionalName ?? folder.Name);
            if (reset)
            {
                config.Reset();
            }
            return config;
        }

        public static FileStorageConfig CreateConfig(DirectoryInfo info, string optionalName = null)
        {
            return new FileStorageConfig(info, optionalName ?? info.Name);
        }

        public static FilePartitionInbox CreateInbox(this FileStorageConfig cfg, string name,
            Func<uint, TimeSpan> decay = null)
        {
            var reader = new StatelessFileQueueReader(Path.Combine(cfg.FullPath, name), name);

            var waiter = decay ?? DecayEvil.BuildExponentialDecay(250);
            var inbox = new FilePartitionInbox(new[] {reader,}, waiter);
            inbox.Init();
            return inbox;
        }

        public static FileQueueWriter CreateQueueWriter(this FileStorageConfig cfg, string queueName)
        {
            var full = Path.Combine(cfg.Folder.FullName, queueName);
            if (!Directory.Exists(full))
            {
                Directory.CreateDirectory(full);
            }
            return
                new FileQueueWriter(new DirectoryInfo(full), queueName);
        }

        public static SimpleMessageSender CreateSimpleSender(this FileStorageConfig account, IEnvelopeStreamer streamer,
            string queueName)
        {
            return new SimpleMessageSender(streamer, CreateQueueWriter(account, queueName));
        }
    }
}