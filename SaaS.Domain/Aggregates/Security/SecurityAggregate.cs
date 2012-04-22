#region (c) 2010-2012 Lokad - CQRS Sample for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;

namespace Sample.Aggregates.Security
{
    public sealed class SecurityAggregate
    {
        readonly SecurityState _state;

        public IList<IEvent<IIdentity>> Changes = new List<IEvent<IIdentity>>();

        public SecurityAggregate(SecurityState state)
        {
            _state = state;
        }


        void Apply(IEvent<SecurityId> e)
        {
            _state.Mutate(e);
            Changes.Add(e);
        }

        public void AddPassword(IDomainIdentityService ids, IUserIndexService index, PasswordGenerator pwds,
            string display, string login, string password)
        {
            if (index.IsLoginRegistered(login))
                throw DomainError.Named("duplicate-login", "Login {0} is already taken", login);

            var user = new UserId(ids.GetId());
            var salt = pwds.CreateSalt();
            var token = pwds.CreateToken();
            var hash = pwds.HashPassword(password, salt);
            Apply(new SecurityPasswordAdded(_state.Id, user, display, login, hash, salt, token));
        }

        public void AddIdentity(IDomainIdentityService ids, PasswordGenerator pwds, string display, string identity)
        {
            var user = new UserId(ids.GetId());
            var token = pwds.CreateToken();
            Apply(new SecurityIdentityAdded(_state.Id, user, display, identity, token));
        }


        public void CreateSecurityAggregate(SecurityId securityId)
        {
            Apply(new SecurityAggregateCreated(securityId));
        }

        public void When(IDomainIdentityService ids, PasswordGenerator pwds, CreateSecurityFromRegistration c)
        {
            Apply(new SecurityAggregateCreated(c.Id));

            var user = new UserId(ids.GetId());
            var salt = pwds.CreateSalt();
            var token = pwds.CreateToken();
            var hash = pwds.HashPassword(c.Pwd, salt);

            Apply(new SecurityPasswordAdded(c.Id, user, c.DisplayName, c.Login, hash, salt, token));
            if (!string.IsNullOrEmpty(c.OptionalIdentity))
            {
                AddIdentity(ids, pwds, c.DisplayName, c.OptionalIdentity);
            }
            Apply(new SecurityRegistrationProcessCompleted(c.Id, c.DisplayName, user, token, c.RegistrationId));
        }

        public void RemoveSecurityItem(UserId userId)
        {
            SecurityState.User user;
            if (!_state.TryGetUser(userId, out user))
            {
                throw new InvalidOperationException("User not found");
            }
            var s = user.Kind.ToString().ToLowerInvariant();
            Apply(new SecurityItemRemoved(_state.Id, user.Id, user.Lookup, s));
        }


        public void UpdateDisplayName(UserId userId, string displayName)
        {
            var user = _state.GetUser(userId);

            if (user.DisplayName == displayName)
                return;

            Apply(new SecurityItemDisplayNameUpdated(_state.Id, userId, displayName));
        }

        public void AddPermissionToSecurityItem(UserId userId, string permission)
        {
            if (string.IsNullOrEmpty(permission))
                throw DomainError.Named("empty", "Permission can't be empty");

            if (!_state.ContainsUser(userId))
            {
                throw DomainError.Named("invalid-user", "User {0} does not exist", userId.Id);
            }

            var user = _state.GetUser(userId);
            if (!user.Permissions.Contains(permission))
                Apply(new PermissionAddedToSecurityItem(_state.Id, user.Id, user.DisplayName, permission, user.Token));
        }
    }
}