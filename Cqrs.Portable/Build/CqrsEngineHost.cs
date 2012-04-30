#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lokad.Cqrs.Build.Events;

namespace Lokad.Cqrs.Build
{
    public sealed class CqrsEngineHost : IDisposable
    {
        readonly ICollection<IEngineProcess> _serverProcesses;

        readonly Stack<IDisposable> _disposables = new Stack<IDisposable>();

        public CqrsEngineHost(ICollection<IEngineProcess> serverProcesses)
        {
            _serverProcesses = serverProcesses;

            foreach (var engineProcess in serverProcesses)
            {
                _disposables.Push(engineProcess);
            }
        }

        public void AddDisposable(IDisposable disposable)
        {
            _disposables.Push(disposable);
        }

        public void RunForever()
        {
            var token = new CancellationTokenSource();
            Start(token.Token);
            token.Token.WaitHandle.WaitOne();
        }

        public Task Start(CancellationToken token)
        {
            var tasks = _serverProcesses.Select(p => p.Start(token)).ToArray();

            if (tasks.Length == 0)
            {
                throw new InvalidOperationException(string.Format("There were no instances of '{0}' registered",
                    typeof(IEngineProcess).Name));
            }

            var names =
                _serverProcesses.Select(p => string.Format("{0}({1:X8})", p.GetType().Name, p.GetHashCode())).ToArray();

            SystemObserver.Notify(new EngineStarted(names));

            return Task.Factory.StartNew(() =>
                {
                    var watch = Stopwatch.StartNew();
                    try
                    {
                        Task.WaitAll(tasks, token);
                    }
                    catch (OperationCanceledException) {}
                    SystemObserver.Notify(new EngineStopped(watch.Elapsed));
                });
        }


        internal void Initialize()
        {
            SystemObserver.Notify(new EngineInitializationStarted());
            foreach (var process in _serverProcesses)
            {
                process.Initialize();
            }
            SystemObserver.Notify(new EngineInitialized());
        }


        public void Dispose()
        {
            while (_disposables.Count > 0)
            {
                try
                {
                    _disposables.Pop().Dispose();
                }
                catch {}
            }
        }
    }
}