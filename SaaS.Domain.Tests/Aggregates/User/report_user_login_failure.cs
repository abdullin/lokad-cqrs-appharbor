#region (c) 2010-2012 Lokad - CQRS Sample for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Sample;

// ReSharper disable InconsistentNaming

namespace SaaS.Aggregates.User
{
    public class report_user_login_failure : specs
    {
        static readonly UserId id = new UserId(1);
        static readonly SecurityId sec = new SecurityId(1);

        static IEvent<UserId> Failure(int minutes)
        {
            return new UserLoginFailureReported(id, Time(1, minutes), sec, "local");
        }

        static readonly TimeSpan fiveMins = TimeSpan.FromMinutes(5);


        public spec given_five_failures_within_threshold_should_lock = new user_spec
            {
                Given =
                    {
                        new UserCreated(id, sec, fiveMins),
                        Failure(01),
                        Failure(02),
                        Failure(03),
                        Failure(03),
                    },
                When = new ReportUserLoginFailure(id, Time(1, 04), "local"),
                Expect =
                    {
                        Failure(04),
                        new UserLocked(id, "Login failed too many times", sec, Time(1, 14))
                    }
            };

        public spec given_five_failures_outside_threshold_should_do_nothing = new user_spec
            {
                Given =
                    {
                        new UserCreated(id, sec, fiveMins),
                        Failure(01),
                        Failure(02),
                        Failure(03),
                        Failure(03),
                    },
                When = new ReportUserLoginFailure(id, Time(1, 07), "local"),
                Expect =
                    {
                        Failure(07),
                    }
            };

        public spec five_failures_lock_and_unlock = new user_spec
            {
                Given =
                    {
                        new UserCreated(id, sec, fiveMins),
                        Failure(1),
                        Failure(2),
                        Failure(3),
                        Failure(3),
                        Failure(3),
                        new UserLocked(id, "test", sec, Time(1, 13)),
                        new UserUnlocked(id, "test", sec)
                    },
                When = new ReportUserLoginFailure(id, Time(1, 04), "local"),
                Expect =
                    {
                        Failure(4)
                    }
            };
    }
}