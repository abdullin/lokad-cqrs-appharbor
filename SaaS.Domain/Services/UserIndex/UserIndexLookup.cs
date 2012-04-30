#region (c) 2010-2012 Lokad - CQRS Sample for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SaaS.Services.UserIndex
{
    [DataContract]
    public sealed class UserIndexLookup
    {
        [DataMember(Order = 1)]
        public IDictionary<string, long> Logins { get; private set; }

        [DataMember(Order = 3)]
        public IDictionary<string, long> Identities { get; private set; }

        public UserIndexLookup()
        {
            Logins = new Dictionary<string, long>(StringComparer.InvariantCultureIgnoreCase);
            Identities = new Dictionary<string, long>();
        }
    }
}