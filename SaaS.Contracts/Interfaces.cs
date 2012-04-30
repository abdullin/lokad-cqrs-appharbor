#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Collections.Generic;

namespace SaaS
{
    public interface ISampleMessage {}

    public interface ISampleCommand : ISampleMessage {}

    public interface ISampleEvent : ISampleMessage {}

    public interface ICommand<out TIdentity> : ISampleCommand
        where TIdentity : IIdentity
    {
        TIdentity Id { get; }
    }

    public interface IApplicationService
    {
        void Execute(object command);
    }


    public interface IEvent<out TIdentity> : ISampleEvent
        where TIdentity : IIdentity
    {
        TIdentity Id { get; }
    }

    public interface IFunctionalCommand : ISampleCommand {}

    public interface IFunctionalEvent : ISampleEvent {}

    /// <summary>The only messaging endpoint that is available to stateless services
    /// They are not allowed to send any other messages.</summary>
    public interface IEventPublisher
    {
        void Publish(IFunctionalEvent notification);
        void PublishBatch(IEnumerable<IFunctionalEvent> events);
    }


    public interface IAggregate<in TIdentity>
        where TIdentity : IIdentity
    {
        void Execute(ICommand<TIdentity> c);
    }


    public interface IAggregateState
    {
        void Apply(IEvent<IIdentity> e);
    }


    /// <summary>
    /// Semi strongly-typed message sending endpoint made
    ///  available to stateless workflow processes.
    /// </summary>
    public interface ICommandSender
    {
        /// <summary>
        /// This interface is intentionally made long and unusable. Generally within the domain 
        /// (as in Mousquetaires domain) there will be extension methods that provide strongly-typed
        /// extensions (that don't allow sending wrong command to wrong location).
        /// </summary>
        /// <param name="commands">The commands.</param>
        void SendCommandsAsBatch(ISampleCommand[] commands);
    }


    public interface IEventStore
    {
        EventStream LoadEventStream(IIdentity id);
        void AppendToStream(IIdentity id, long version, ICollection<IEvent<IIdentity>> events, string explanation);
    }

    public sealed class EventStream
    {
        public long Version;
        public IList<IEvent<IIdentity>> Events;
    }
}