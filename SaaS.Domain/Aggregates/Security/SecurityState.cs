#region (c) 2010-2012 Lokad - CQRS Sample for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;

namespace SaaS.Aggregates.Security
{
    public sealed class SecurityState : ISecurityState
    {
        public sealed class User
        {
            public string DisplayName { get; set; }
            public bool Removed { get; set; }
            public UserId Id { get; set; }
            public RegistrationId Registration { get; set; }
            public string Token { get; set; }
            public bool Locked { get; set; }
            public readonly string Lookup;
            public readonly SecurityItemType Kind;

            public HashSet<string> Permissions = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

            public User(string lookup, SecurityItemType kind)
            {
                Lookup = lookup;
                Kind = kind;
            }
        }

        public enum SecurityItemType
        {
            Undefined,
            Key,
            Password,
            Identity
        }


        readonly IDictionary<UserId, User> _globals = new Dictionary<UserId, User>();


        public SecurityId Id { get; private set; }

        public User GetUser(UserId userNum)
        {
            return _globals[userNum];
        }


        public bool TryGetUser(UserId id, out User user)
        {
            return _globals.TryGetValue(id, out user);
        }

        public SecurityState(IEnumerable<IEvent<IIdentity>> events)
        {
            foreach (var e in events)
            {
                Mutate(e);
            }
        }

        public bool ContainsUser(UserId id)
        {
            return _globals.ContainsKey(id);
        }

        public sealed class Invite
        {
            public string Email { get; private set; }
            public string DisplayName { get; private set; }
            public UserId FutureUserId { get; private set; }
            public string FutureToken { get; private set; }

            public Invite(string email, string displayName, UserId userId, string token)
            {
                Email = email;
                DisplayName = displayName;
                FutureUserId = userId;
                FutureToken = token;
            }
        }

        IDictionary<string, Invite> _invites = new Dictionary<string, Invite>(StringComparer.InvariantCultureIgnoreCase);

        public bool TryGetInvite(string invite, out Invite result)
        {
            return _invites.TryGetValue(invite, out result);
        }

        public void Mutate(IEvent<IIdentity> e)
        {
            RedirectToWhen.InvokeEventOptional(this, e);
        }

        public void When(SecurityItemRemoved e)
        {
            _globals[e.UserId].Removed = true;
        }

        public void When(SecurityItemDisplayNameUpdated e)
        {
            _globals[e.UserId].DisplayName = e.DisplayName;
        }

        public void When(SecurityRegistrationProcessCompleted c) {}

        public void When(PermissionAddedToSecurityItem c)
        {
            _globals[c.UserId].Permissions.Add(c.Permission);
        }

        public void When(SecurityAggregateCreated e)
        {
            Id = e.Id;
        }

        public void When(SecurityIdentityAdded e)
        {
            var user = new User(e.Identity, SecurityItemType.Identity)
                {
                    Id = e.UserId,
                    DisplayName = e.DisplayName,
                    Token = e.Token
                };
            _globals.Add(e.UserId, user);
        }

        public void When(SecurityPasswordAdded e)
        {
            var user = new User(e.Login, SecurityItemType.Password)
                {
                    Id = e.UserId,
                    DisplayName = e.DisplayName,
                    Token = e.Token
                };
            _globals.Add(e.UserId, user);
        }
    }
}