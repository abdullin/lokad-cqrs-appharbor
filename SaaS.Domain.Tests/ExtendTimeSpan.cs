#region (c) 2010-2012 Lokad - CQRS Sample for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;

namespace SaaS
{
    public static class ExtendTimeSpan
    {
        public static TimeSpan Days(this int days)
        {
            return TimeSpan.FromDays(days);
        }
    }
}