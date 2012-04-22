#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Lokad.Cqrs.Envelope
{
    ///<summary>
    /// Shoud be registered as singleton, manages actual memories
    /// and performs cleanups in async
    ///</summary>
    public sealed class MessageDuplicationManager : IEngineProcess
    {
        readonly ConcurrentDictionary<object, MessageDuplicationMemory> _memories =
            new ConcurrentDictionary<object, MessageDuplicationMemory>();

        public void Dispose() {}

        public void Initialize() {}

        public MessageDuplicationMemory GetOrAdd(object dispatcher)
        {
            return _memories.GetOrAdd(dispatcher, s => new MessageDuplicationMemory());
        }

        public Task Start(CancellationToken token)
        {
            return Task.Factory.StartNew(() =>
                {
                    while (!token.IsCancellationRequested)
                    {
                        foreach (var memory in _memories)
                        {
                            memory.Value.ForgetOlderThan(TimeSpan.FromMinutes(20));
                        }

                        token.WaitHandle.WaitOne(TimeSpan.FromMinutes(5));
                    }
                }, token);
        }
    }
}