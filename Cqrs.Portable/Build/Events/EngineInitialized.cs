#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;

namespace SaaS.Build.Events
{
    [Serializable]
    public sealed class EngineInitialized : ISystemEvent
    {
        public override string ToString()
        {
            return "Engine initialized";
        }
    }
}