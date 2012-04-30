using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Lokad.Cqrs
{
    public static class TestObserver
    {
        sealed class Disposable : IDisposable
        {
            Action _action;
            public Disposable(Action action)
            {
                _action = action;
            }

            public void Dispose()
            {
                _action();
            }
        }

        sealed class EventsObserver : IObserver<ISystemEvent>
        {
            readonly Action<ISystemEvent> _when;
            public EventsObserver(Action<ISystemEvent> when)
            {
                _when = when;
            }

            public void OnNext(ISystemEvent value)
            {
                _when(value);
            }

            public void OnError(Exception error)
            {
            }

            public void OnCompleted()
            {
            }
        }

        public static IDisposable When<T>(Action<T> when, bool includeTracing = true) where T : class
        {
            var observer = new EventsObserver(@event =>
                {
                    var type1 = @event as T;

                    if (type1 != null)
                    {
                        when(type1);
                    }
                    if (includeTracing)
                    {
                        Trace.WriteLine(@event);
                    }
                });
           
            var old = SystemObserver.Swap(new IObserver<ISystemEvent>[]{observer});

            return new Disposable(() => SystemObserver.Swap(old));
        }
    }
}