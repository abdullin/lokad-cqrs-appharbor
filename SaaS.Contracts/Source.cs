#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Linq;

namespace Sample
{
    public sealed class Source<TEvent> where TEvent : ISampleEvent
    {
        public readonly string MessageId;
        public readonly DateTime CreatedUtc;
        public readonly TEvent Event;

        public Source(string messageId, DateTime createdUtc, TEvent @event)
        {
            MessageId = messageId;
            CreatedUtc = createdUtc;
            Event = @event;
        }
    }

    public static class Source
    {
        public static object For(string messageId, DateTime date, ISampleEvent instance)
        {
            return typeof(Source<>).MakeGenericType(instance.GetType()).GetConstructors()
                .First()
                .Invoke(new object[] {messageId, date, instance});
        }
    }
}