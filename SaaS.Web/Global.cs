#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using Lokad.Cqrs;
using SaaS.Wires;

namespace SaaS.Web
{
    public static class Global
    {
        static readonly FileStorageConfig Root;
        //public static readonly HubClient Client;

        public static readonly FormsAuth Forms;
        public static readonly AzureAuth Auth;


        static Global()
        {
            Root = FileStorage.CreateConfig(@"C:\data\hub-store");

            var streamer = Contracts.CreateStreamer();
            //var routerQueue = Root.CreateQueueWriter(Topology.RouterQueue);
            //var nuclearStorage = Root.CreateNuclear(new DocumentStrategy());

            //Client = new HubClient(nuclearStorage, streamer, routerQueue);

            //Forms = new FormsAuth(nuclearStorage.Container.GetReader<UserId, LoginView>());
            //Auth = new AzureAuth(Client);
        }
    }
}