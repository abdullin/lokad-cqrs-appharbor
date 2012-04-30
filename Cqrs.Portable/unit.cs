#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Runtime.InteropServices;

namespace SaaS
{
    /// <summary>
    /// Equivalent to System.Void which is not allowed to be used in the code for some reason.
    /// </summary>
    [ComVisible(true)]
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Size = 1)]
    public struct unit
    {
        public static readonly unit it = default(unit);
    }
}