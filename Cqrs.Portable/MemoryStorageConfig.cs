#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Lokad.Cqrs
{
    public sealed class MemoryStorageConfig : HideObjectMembersFromIntelliSense
    {
        public readonly ConcurrentDictionary<string, BlockingCollection<byte[]>> Queues =
            new ConcurrentDictionary<string, BlockingCollection<byte[]>>();

        public readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte[]>> Data =
            new ConcurrentDictionary<string, ConcurrentDictionary<string, byte[]>>();

        public readonly ConcurrentDictionary<string, List<byte[]>> Tapes =
            new ConcurrentDictionary<string, List<byte[]>>();
    }
}