#region (c) 2010-2012 Lokad - CQRS Sample for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Sample;

// ReSharper disable InconsistentNaming

namespace SaaS.Aggregates.User
{
    public class unlock_user : specs
    {
        static readonly UserId id = new UserId(1);
        static readonly SecurityId sec = new SecurityId(1);
        static readonly TimeSpan fiveMins = TimeSpan.FromMinutes(5);


        public spec given_locked_user = new user_spec
            {
                Given =
                    {
                        new UserCreated(id, sec, fiveMins),
                        new UserLocked(id, "locked", sec, Current.MaxValue)
                    },
                When = new UnlockUser(id, "reason"),
                Expect = {new UserUnlocked(id, "reason", sec)},
            };


        public spec given_new_user = new user_spec
            {
                Given = {new UserCreated(id, sec, fiveMins)},
                When = new UnlockUser(id, "reason"),
            };
    }
}