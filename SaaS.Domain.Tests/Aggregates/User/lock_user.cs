#region (c) 2010-2012 Lokad - CQRS Sample for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Sample;

// ReSharper disable InconsistentNaming

namespace SaaS.Aggregates.User
{
    public class lock_user : specs
    {
        static readonly UserId id = new UserId(1);
        static readonly SecurityId sec = new SecurityId(1);
        static readonly TimeSpan fiveMins = TimeSpan.FromMinutes(5);

        public spec given_new_user = new user_spec
            {
                Given = {new UserCreated(id, sec, fiveMins)},
                When = new LockUser(id, "reason"),
                Expect = {new UserLocked(id, "reason", sec, Current.MaxValue)},
            };

        public spec given_locked_user = new user_spec
            {
                Given =
                    {
                        new UserCreated(id, sec, fiveMins),
                        new UserLocked(id, "locked", sec, Current.MaxValue)
                    },
                When = new LockUser(id, "lock again")
            };

        public spec given_temporarily_locked_user = new user_spec
            {
                Given =
                    {
                        new UserCreated(id, sec, fiveMins),
                        new UserLocked(id, "locked", sec, Time(1, 20))
                    },
                When = new LockUser(id, "lock again"),
                Expect =
                    {
                        new UserLocked(id, "lock again", sec, Current.MaxValue)
                    }
            };

        public spec given_no_user = new user_fail
            {
                When = new LockUser(id, "Reason"),
                Expect = {error => error.Name == "premature"}
            };

        public spec given_deleted_user = new user_fail
            {
                Given =
                    {
                        new UserCreated(id, sec, TimeSpan.FromMinutes(5)),
                        new UserDeleted(id, sec)
                    },
                When = new LockUser(id, "sec"),
                Expect = {error => error.Name == "zombie"}
            };
    }
}