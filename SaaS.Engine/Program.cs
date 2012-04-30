using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Threading;
using Lokad.Cqrs;
using SaaS.Wires;
using ServiceStack.Text;

namespace SaaS.Engine
{
    class Program
    {
        static void Main(string[] args)
        {
            

            ConfigureObserver();

            var settings = LoadSettings();

            foreach (var setting in settings)
            {
                SystemObserver.Notify("[{0}] = {1}", setting.Key, setting.Value);
            }

            var setup = new SetupClassThatReplacesIoCContainerFramework();
            
            var integrationPath = settings["DataPath"];
            if (integrationPath.StartsWith("file:"))
            {
                var path = integrationPath.Remove(0, 5);
                var config = FileStorage.CreateConfig(path);
                setup.Streaming = config.CreateStreaming();
                setup.Tapes = config.CreateTape;
                setup.Docs = config.CreateNuclear(setup.Strategy).Container;
                setup.CreateInbox = s => config.CreateInbox(s);
                setup.CreateQueueWriter = config.CreateQueueWriter;
            }
            else if (integrationPath.StartsWith("azure:"))
            {
                var path = integrationPath.Remove(0, 6);
                var config = AzureStorage.CreateConfig(path);
                setup.Streaming = config.CreateStreaming();
                setup.Tapes = config.CreateTape;
                setup.Docs = config.CreateNuclear(setup.Strategy).Container;
                setup.CreateInbox = s => config.CreateInbox(s);
                setup.CreateQueueWriter = config.CreateQueueWriter;
            }
            else
            {
                throw new InvalidOperationException("Unsupperted environment");
            }
            var components = setup.AssembleComponents();


            var stream = setup.Tapes(Topology.TapesContainer).GetOrCreateStream(Topology.DomainLogName);

            StartupProjectionRebuilder.Rebuild(components.Setup.Docs, stream);
            using (var cts = new CancellationTokenSource())
            using (var engine = components.Builder.Build())
            {
                var task = engine.Start(cts.Token);

                var version = ConfigurationManager.AppSettings.Get("appharbor.commit_id");
                var instanceStarted = new InstanceStarted(version, "engine", Process.GetCurrentProcess().ProcessName);
                components.Simple.SendOne(instanceStarted);

                //startupMessages.ForEach(c => components.Sender.SendCommandsAsBatch(new ISampleCommand[] { c }));

                Console.WriteLine(@"Press enter to stop");
                Console.ReadLine();
                cts.Cancel();
                if (!task.Wait(5000))
                {
                    Console.WriteLine(@"Terminating");
                }
            }
        }

        static void ConfigureObserver()
        {
            Trace.Listeners.Add(new ConsoleTraceListener());

            var observer = new ConsoleObserver();
            SystemObserver.Swap(observer);
            Context.SwapForDebug(s => SystemObserver.Notify(s));
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
