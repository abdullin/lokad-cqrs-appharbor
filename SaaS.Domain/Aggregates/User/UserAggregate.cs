#region (c) 2010-2012 Lokad - CQRS Sample for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;

namespace SaaS.Aggregates.User
{
    public sealed class UserAggregate
    {
        readonly UserState _state;
        public IList<IEvent<IIdentity>> Changes = new List<IEvent<IIdentity>>();

        /// <summary>
        /// If relogin happens within the interval, we don't track it
        /// </summary>
        public static readonly TimeSpan DefaultLoginActivityThreshold = TimeSpan.FromMinutes(10);

        public UserAggregate(UserState state)
        {
            _state = state;
        }

        public void Create(UserId userId, SecurityId securityId)
        {
            if (_state.Version != 0)
                throw new DomainError("User already has non-zero version");

            Apply(new UserCreated(userId, securityId, DefaultLoginActivityThreshold));
        }

        public void ReportLoginFailure(DateTime timeUtc, string address)
        {
            Apply(new UserLoginFailureReported(_state.Id, timeUtc, _state.SecurityId, address));
            if (_state.DoesLastFailureWarrantLockout())
            {
                Apply(new UserLocked(_state.Id, "Login failed too many times", _state.SecurityId, timeUtc.AddMinutes(10)));
            }
        }

        public void ReportLoginSuccess(DateTime timeUtc, string address)
        {
            Apply(new UserLoginSuccessReported(_state.Id, timeUtc, _state.SecurityId, address));
        }

        public void Unlock(string unlockReason)
        {
            if (Current.UtcNow > _state.LockedOutTillUtc)
                return; // lock has already expired

            Apply(new UserUnlocked(_state.Id, unlockReason, _state.SecurityId));
        }

        public void Delete()
        {
            Apply(new UserDeleted(_state.Id, _state.SecurityId));
        }

        public void Lock(string lockReason)
        {
            if (_state.LockedOutTillUtc == Current.MaxValue)
                return;

            Apply(new UserLocked(_state.Id, lockReason, _state.SecurityId, Current.MaxValue));
        }

        public void ThrowOnInvalidStateTransition(ICommand<UserId> e)
        {
            if (_state.Version == 0)
            {
                if (e is CreateUser)
                {
                    return;
                }
                throw DomainError.Named("premature", "Can't do anything to unexistent aggregate");
            }
            if (_state.Version == -1)
            {
                throw DomainError.Named("zombie", "Can't do anything to deleted aggregate.");
            }
            if (e is CreateUser)
                throw DomainError.Named("rebirth", "Can't create aggregate that already exists");
        }

        void Apply(IEvent<UserId> e)
        {
            _state.Mutate(e);
            Changes.Add(e);
        }
    }
}