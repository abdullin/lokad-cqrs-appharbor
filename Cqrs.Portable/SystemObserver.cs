#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Diagnostics;
using System.Globalization;

namespace SaaS
{
    public static class SystemObserver
    {
        static IObserver<ISystemEvent>[] _observers = new IObserver<ISystemEvent>[0];

        sealed class ActionObserver : IObserver<ISystemEvent>
        {
            readonly Action<ISystemEvent> _action;

            public ActionObserver(Action<ISystemEvent> action)
            {
                _action = action;
            }

            public void OnNext(ISystemEvent value)
            {
                _action(value);
            }

            public void OnError(Exception error) {}

            public void OnCompleted() {}
        }

        public static IObserver<ISystemEvent>[] Swap(params IObserver<ISystemEvent>[] swap)
        {
            var old = _observers;
            _observers = swap;
            return old;
        }

        public static void Put(Action<ISystemEvent> se)
        {
            _observers = new IObserver<ISystemEvent>[] {new ActionObserver(se),};
        }

        public static void Notify(ISystemEvent @event)
        {
            foreach (var observer in _observers)
            {
                try
                {
                    observer.OnNext(@event);
                }
                catch (Exception ex)
                {
                    var message = string.Format("Observer {0} failed with {1}", observer, ex);
                    Trace.WriteLine(message);
                }
            }
        }

        public static void Notify(string message, params object[] args)
        {
            Notify(new MessageEvent(string.Format(CultureInfo.InvariantCulture, message, args)));
        }

        public sealed class MessageEvent : ISystemEvent
        {
            public MessageEvent(string message)
            {
                Message = message;
            }

            public readonly string Message;

            public override string ToString()
            {
                return Message;
            }
        }


        public static void Complete()
        {
            foreach (var observer in _observers)
            {
                observer.OnCompleted();
            }
        }
    }
}