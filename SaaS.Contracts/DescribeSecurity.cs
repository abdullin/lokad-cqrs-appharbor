#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

namespace Sample
{
    static class DescribeSecurity
    {
        static string When(CreateSecurityAggregate c)
        {
            return "Create security group";
        }

        static string When(SecurityAggregateCreated e)
        {
            return "Security group created";
        }

        static string When(AddSecurityPassword e)
        {
            return string.Format("Add login '{0}': {1}/{2}", e.DisplayName, e.Login, e.Password);
        }

        static string When(SecurityPasswordAdded e)
        {
            return string.Format("Added login '{0}' as {1} with encrypted pass and salt", e.DisplayName, e.UserId.Id);
        }

        static string When(AddSecurityIdentity c)
        {
            return string.Format("Add identity '{0}': {1}", c.DisplayName, c.Identity);
        }

        static string When(SecurityIdentityAdded e)
        {
            return string.Format("Added identity '{0}' as {1}", e.DisplayName, e.UserId.Id);
        }
    }
}