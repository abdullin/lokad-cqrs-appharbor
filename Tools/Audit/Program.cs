#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.IO;
using System.Windows.Forms;
using Audit.Util;
using Audit.Views;
using Lokad.Cqrs;
using Lokad.Cqrs.TapeStorage;
using SaaS;
using SaaS.Wires;

namespace Audit
{
    static class Program
    {
        static readonly IEnvelopeStreamer EnvelopeStreamer;

        static Program()
        {
            EnvelopeStreamer = Contracts.CreateStreamer();
        }

        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (var form = new LoadSettingsForm())
            {
                form.BindArgs(args);
                if (form.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                var conf = form.GetAzureConfig();
                if (form.ConfigFileSelected)
                {
                    AttachToRemoteLog(conf);
                }
                else
                {
                    AttachToLocalLog(conf);
                }
            }
        }

        static void AttachToLocalLog(string filePath)
        {
            SimpleMessageSender sender;
            var cache = new FileTapeStream(filePath);
            var store = new LocalEventStore(null, cache);

            var directoryName = Path.GetDirectoryName(filePath) ?? "";
            var dir = new DirectoryInfo(directoryName);
            FileStorageConfig config;

            if (dir.Name == Topology.TapesContainer)
            {
                // we are in proper tapes container
                config = FileStorage.CreateConfig(dir.Parent);
            }
            else
            {
                var temp = Path.Combine(dir.FullName, string.Format("temp-{0:yyyy-MM-dd-HHmm}", DateTime.Now));
                config = FileStorage.CreateConfig(temp);
            }

            var send = config.CreateQueueWriter(Topology.RouterQueue);
            var endpoint = new SimpleMessageSender(EnvelopeStreamer, send);
            Application.Run(new DomainLogView(store, endpoint, EnvelopeStreamer));
        }

        static void AttachToRemoteLog(string azureConfig)
        {
            throw new NotImplementedException();
            //var config = AzureStorage.CreateConfig(azureConfig);

            //var streamFactory = new BlockBlobTapeStorageFactory(config, Topology.TapeContainer);
            //var tapeStream = streamFactory.GetOrCreateStream(Topology.DomainStream);
            //var cache = CreateCache(config);
            //var store = new LocalEventStore(tapeStream, cache);
            //var azureQueueWriterFactory = new AzureQueueWriterFactory(config, EnvelopeStreamer);

            //var endpoint = new InboxEndpoint(azureQueueWriterFactory.GetWriteQueue(Topology.InboxQueues[0]));
            //Application.Run(new DomainLogView(store, endpoint, EnvelopeStreamer));
        }

        //public static FileTapeStream CreateCache(IAzureStorageConfig config)
        //{
        //    var folderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        //    var name = String.Format("shelfcheck_fs_{0:X8}.tmd", config.AccountName.GetHashCode());

        //    var writerFactory = new FileTapeStorageFactory(folderPath);
        //    writerFactory.InitializeForWriting();

        //    return (FileTapeStream) writerFactory.GetOrCreateStream(name);
        //}
    }
}