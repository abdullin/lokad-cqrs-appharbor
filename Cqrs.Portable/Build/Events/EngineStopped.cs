﻿#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;

namespace Lokad.Cqrs.Build.Events
{
    [Serializable]
    public sealed class EngineStopped : ISystemEvent
    {
        public TimeSpan Elapsed { get; private set; }

        public EngineStopped(TimeSpan elapsed)
        {
            Elapsed = elapsed;
        }

        public override string ToString()
        {
            return string.Format("Engine Stopped after {0} mins", Math.Round(Elapsed.TotalMinutes, 2));
        }
    }
}