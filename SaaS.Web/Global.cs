#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Configuration;
using Lokad.Cqrs;
using Lokad.Cqrs.AtomicStorage;
using SaaS.AtomicStorage;
using SaaS.Client;
using SaaS.Client.Projections.LoginView;
using SaaS.Wires;

namespace SaaS.Web
{
    public static class Global
    {
        static readonly FileStorageConfig Root;
        //public static readonly HubClient Client;
        public static readonly WebEndpoint Client;
        public static readonly FormsAuth Forms;
        public static readonly WebAuth Auth;
        public static readonly string CommitId;
        public static readonly IDocumentStore Docs;
        
        static Global()
        {
            CommitId = ConfigurationManager.AppSettings.Get("appharbor.commit_id");

            var settings = LoadSettings();

            var integrationPath = settings["DataPath"];
            var contracts = Contracts.CreateStreamer();
            var strategy = new DocumentStrategy();
            if (integrationPath.StartsWith("file:"))
            {
                var path = integrationPath.Remove(0, 5);
                var config = FileStorage.CreateConfig(path);
                
                Docs = config.CreateNuclear(strategy).Container;
                Client = new WebEndpoint(new NuclearStorage(Docs), contracts, config.CreateQueueWriter(Topology.RouterQueue));
            }
            else if (integrationPath.StartsWith("azure:"))
            {
                var path = integrationPath.Remove(0, 6);
                var config = AzureStorage.CreateConfig(path);
                Docs = config.CreateNuclear(strategy).Container;
                Client = new WebEndpoint(new NuclearStorage(Docs), contracts, config.CreateQueueWriter(Topology.RouterQueue));
            }
            else
            {
                throw new InvalidOperationException("Unsupperted environment");
            }

         
         

            Forms = new FormsAuth(Docs.GetReader<UserId, LoginView>());
            Auth = new WebAuth(Client);
        }

        static Dictionary<string, string> LoadSettings()
        {
            var settings = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            foreach (var setting in ConfigurationManager.AppSettings.AllKeys)
            {
                settings[setting] = ConfigurationManager.AppSettings[setting];
            }
            return settings;

        }

    }
}