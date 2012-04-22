#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;

namespace Lokad.Cqrs.Build.Events
{
    [Serializable]
    public sealed class EngineStarted : ISystemEvent
    {
        public readonly string[] EngineProcesses;

        public EngineStarted(string[] engineProcesses)
        {
            EngineProcesses = engineProcesses;
        }

        public override string ToString()
        {
            return string.Format("Engine started: {0}", string.Join(",", EngineProcesses));
        }
    }
}