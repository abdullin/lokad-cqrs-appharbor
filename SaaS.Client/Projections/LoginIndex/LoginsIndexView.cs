#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SaaS.Client
{
    [DataContract(Name = "new-login-index")]
    public sealed class LoginsIndexView
    {
        [DataMember(Order = 1)]
        public IDictionary<string, long> Logins { get; private set; }

        [DataMember(Order = 2)]
        public IDictionary<string, long> Keys { get; private set; }

        [DataMember(Order = 3)]
        public IDictionary<string, long> Identities { get; private set; }

        public bool ContainsIdentty(string identity)
        {
            return Identities.ContainsKey(identity);
        }

        public bool ContainsLogin(string login)
        {
            return Logins.ContainsKey(login);
        }

        public LoginsIndexView()
        {
            Logins = new Dictionary<string, long>(StringComparer.InvariantCultureIgnoreCase);
            Keys = new Dictionary<string, long>(StringComparer.InvariantCultureIgnoreCase);
            Identities = new Dictionary<string, long>(StringComparer.InvariantCultureIgnoreCase);
        }
    }
}