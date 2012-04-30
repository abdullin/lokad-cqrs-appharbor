#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

namespace SaaS
{
    /// <summary>
    /// Methods to describe messages related to <see cref="IUserAggregate"/>. They are auto-wired by
    /// <see cref="Describe"/>
    /// </summary>
    static class DescribeUser
    {
        static string When(CreateUser e)
        {
            return string.Format("Create user {0} for security {1}", e.Id.Id, e.SecurityId.Id);
        }

        static string When(UserCreated e)
        {
            return string.Format("Created user {0} (security {2}) with threshold {1}", e.Id.Id, e.ActivityThreshold,
                e.SecurityId.Id);
        }

        static string When(LockUser e)
        {
            return string.Format("Lock user {0} with reason '{1}'", e.Id.Id, e.LockReason);
        }

        static string When(UserLocked e)
        {
            return string.Format("User {0} locked with reason '{1}'.", e.Id.Id, e.LockReason);
        }

        static string When(UserDeleted e)
        {
            return string.Format("Deleted user {0} from security {1}", e.Id.Id, e.SecurityId.Id);
        }

        static string When(UserLoginFailureReported e)
        {
            return string.Format("User {0} login failed at {1} (via IP '{2}')", e.Id.Id, e.TimeUtc, e.Ip);
        }

        static string When(UserLoginSuccessReported e)
        {
            return string.Format("User {0} logged in at {1} (via IP '{2}')", e.Id.Id, e.TimeUtc, e.Ip);
        }

        static string When(ReportUserLoginFailure e)
        {
            return string.Format("Report login failure for user {0} at {1}", e.Id.Id, e.TimeUtc);
        }
    }
}