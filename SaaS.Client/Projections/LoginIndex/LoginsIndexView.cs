#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Lokad.Cqrs;
using Lokad.Cqrs.AtomicStorage;

namespace SaaS.Client.Projections.LoginIndex
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

    public sealed class LoginsIndexProjection
    {
        readonly IDocumentWriter<unit, LoginsIndexView> _writer;
        public LoginsIndexProjection(IDocumentWriter<unit, LoginsIndexView> writer)
        {
            _writer = writer;
        }

        public void When(SecurityPasswordAdded e)
        {
            _writer.UpdateEnforcingNew(unit.it, si => si.Logins[e.Login] = e.UserId.Id);
        }
        public void When(SecurityIdentityAdded e)
        {
            _writer.UpdateEnforcingNew(unit.it, si => si.Identities[e.Identity] = e.UserId.Id);
        }

        public void When(SecurityItemRemoved e)
        {
            _writer.UpdateEnforcingNew(unit.it, si =>
            {
                si.Keys.Remove(e.Lookup);
                si.Logins.Remove(e.Lookup);
                si.Identities.Remove(e.Lookup);
            });
        }
    }

}