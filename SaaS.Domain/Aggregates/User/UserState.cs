#region (c) 2010-2012 Lokad - CQRS Sample for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace Sample.Aggregates.User
{
    public sealed class UserState : IUserState
    {
        public SecurityId SecurityId { get; private set; }
        public string LockoutMessage { get; private set; }
        public UserId Id { get; private set; }
        public int FailuresAllowed { get; private set; }
        public TimeSpan FailureLockoutWindow { get; private set; }
        public TimeSpan LoginActivityTrackingThreshold { get; private set; }
        public DateTime LastLoginUtc { get; private set; }
        public List<DateTime> TrackedLoginFailures { get; private set; }
        public DateTime LockedOutTillUtc { get; private set; }
        //public bool Locked { get; private set; }

        public UserState(IEnumerable<IEvent<IIdentity>> events)
        {
            TrackedLoginFailures = new List<DateTime>();
            FailuresAllowed = 5;
            FailureLockoutWindow = TimeSpan.FromMinutes(5);
            // track every login by default
            LoginActivityTrackingThreshold = TimeSpan.FromMinutes(0);

            foreach (var e in events)
            {
                Mutate(e);
            }
        }


        public bool DoesLastFailureWarrantLockout()
        {
            if (TrackedLoginFailures.Count < FailuresAllowed)
                return false;
            if ((TrackedLoginFailures.Last() - TrackedLoginFailures.First()) < FailureLockoutWindow)
                return true;
            return false;
        }

        public void When(UserLoginSuccessReported e)
        {
            TrackedLoginFailures.Clear();
            LastLoginUtc = e.TimeUtc;
        }

        public void When(UserLoginFailureReported e)
        {
            TrackedLoginFailures.Add(e.TimeUtc);
            // we track only X last failures
            while (TrackedLoginFailures.Count > FailuresAllowed)
            {
                TrackedLoginFailures.RemoveAt(0);
            }
        }

        public void When(UserLocked e)
        {
            LockoutMessage = e.LockReason;
            LockedOutTillUtc = e.LockedTillUtc;
        }

        public void When(UserUnlocked e)
        {
            TrackedLoginFailures.Clear();
            LockedOutTillUtc = DateTime.MinValue;
        }

        public void When(UserDeleted c)
        {
            Version = -1;
        }

        public void When(UserCreated e)
        {
            SecurityId = e.SecurityId;
            Id = e.Id;
        }

        public int Version { get; private set; }

        public void Mutate(IEvent<IIdentity> e)
        {
            Version += 1;
            RedirectToWhen.InvokeEventOptional(this, e);
        }
    }
}