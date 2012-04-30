#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Lokad.Cqrs.TapeStorage
{
    public sealed class TapeStorageInitilization : IEngineProcess
    {
        readonly IEnumerable<ITapeContainer> _storage;

        public TapeStorageInitilization(IEnumerable<ITapeContainer> storage)
        {
            _storage = storage;
        }

        public void Dispose() {}

        public void Initialize()
        {
            foreach (var factory in _storage)
            {
                factory.InitializeForWriting();
            }
        }

        public Task Start(CancellationToken token)
        {
            // don't do anything
            return new Task(() => { });
        }
    }
}