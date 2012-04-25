using System;
using System.Collections.Generic;
using Lokad.Cqrs;
using Lokad.Cqrs.AtomicStorage;
using Lokad.Cqrs.Build;
using Lokad.Cqrs.Partition;
using Lokad.Cqrs.StreamingStorage;
using Lokad.Cqrs.TapeStorage;
using SaaS.Client;
using Sample;

namespace SaaS.Wires
{
    public sealed class SetupClassThatReplacesIoCContainerFramework
    {
        public IStreamRoot Streaming;
        public Func<string,ITapeContainer> Tapes;
        public IDocumentStore Docs;

        public Func<string, IQueueWriter> CreateQueueWriter;
        public Func<string, IPartitionInbox> CreateInbox;
        


        public readonly IEnvelopeStreamer Streamer = Contracts.CreateStreamer();
        public readonly IDocumentStrategy Strategy = new DocumentStrategy();

        public sealed class AssembledComponents
        {
            public SetupClassThatReplacesIoCContainerFramework Setup;
            public CqrsEngineBuilder Builder;
            public ICommandSender Sender;
            public SimpleMessageSender Simple;
        }

        public AssembledComponents AssembleComponents()
        {
            // set up all the variables
            
            var routerQueue = CreateQueueWriter(Topology.RouterQueue);

            var commands = new RedirectToCommand();
            var events = new RedirectToDynamicEvent();

            var eventStore = new TapeStreamEventStore(Tapes(Topology.TapesContainer), Streamer, routerQueue);
            var simple = new SimpleMessageSender(Streamer, routerQueue);
            var flow = new CommandSender(simple);
            var builder = new CqrsEngineBuilder(Streamer);

            // route queue infrastructure together
            builder.Handle(CreateInbox(Topology.RouterQueue), Topology.Route(CreateQueueWriter, Streamer, Tapes), "router");
            builder.Handle(CreateInbox(Topology.EntityQueue), em => CallHandlers(commands, em));
            builder.Handle(CreateInbox(Topology.EventsQueue), aem => CallHandlers(events, aem));


            // message wiring magic
            DomainBoundedContext.ApplicationServices(Docs, eventStore).ForEach(commands.WireToWhen);
            DomainBoundedContext.Receptors(flow).ForEach(events.WireToWhen);
            DomainBoundedContext.Projections(Docs).ForEach(events.WireToWhen);
            DomainBoundedContext.Tasks(flow, Docs, false).ForEach(builder.AddTask);

            ClientBoundedContext.Projections(Docs).ForEach(events.WireToWhen);

            return new AssembledComponents
                {
                    Builder = builder,
                    Sender = flow,
                    Setup = this,
                    Simple = simple
                };
        }

        static void CallHandlers(RedirectToDynamicEvent functions, ImmutableEnvelope aem)
        {
            if (aem.Items.Length != 1)
                throw new InvalidOperationException(
                    "Unexpected number of items in envelope that arrived to projections: " +
                        aem.Items.Length);
            // we wire envelope contents to both direct message call and sourced call (with date wrapper)
            var content = aem.Items[0].Content;
            functions.InvokeEvent(content);
            functions.InvokeEvent(Source.For(aem.EnvelopeId, aem.CreatedOnUtc, (ISampleEvent)content));
        }

        static void CallHandlers(RedirectToCommand serviceCommands, ImmutableEnvelope aem)
        {
            var content = aem.Items[0].Content;
            serviceCommands.Invoke(content);
        }
    }

    public static class ExtendArrayEvil
    {
        public static void ForEach<T>(this IEnumerable<T> self, Action<T> action)
        {
            foreach (var variable in self)
            {
                action(variable);
            }
        }
    }

}