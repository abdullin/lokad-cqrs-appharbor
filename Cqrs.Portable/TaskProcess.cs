#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lokad.Cqrs
{
    public sealed class TaskProcess : IEngineProcess
    {
        readonly Func<CancellationToken, Task> _factoryToStartTask;

        public TaskProcess(Func<CancellationToken, Task> factoryToStartTask)
        {
            _factoryToStartTask = factoryToStartTask;
        }

        public void Dispose() {}

        public void Initialize() {}

        public Task Start(CancellationToken token)
        {
            return _factoryToStartTask(token);
        }
    }
}