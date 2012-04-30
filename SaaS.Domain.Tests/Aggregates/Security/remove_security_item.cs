#region (c) 2010-2012 Lokad - CQRS Sample for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using Sample;

namespace SaaS.Aggregates.Security
{
    public class remove_security_item : specs
    {
        public static readonly SecurityId id = new SecurityId(42);
        public static readonly UserId user = new UserId(15);

        public spec given_password = new security_spec
            {
                Given =
                    {
                        new SecurityAggregateCreated(id),
                        new SecurityPasswordAdded(id, new UserId(15), "my pass", "user", "hash", "salt", "generated-32")
                    },
                When = new RemoveSecurityItem(id, user),
                Expect = {new SecurityItemRemoved(id, user, "user", "password")}
            };

        //public spec given_key = new security_spec
        //    {
        //        Given =
        //            {
        //                new SecurityAggregateCreated(id),
        //                new SecurityIdentityAdded(id, new UserId(15), "my key", "legacy-key", "generated-32")
        //            },
        //        When = new RemoveSecurityItem(id, user),
        //        Expect = {new SecurityItemRemoved(id, user, "legacy-key", "key")}
        //    };

        public spec given_identity = new security_spec
            {
                Given =
                    {
                        new SecurityAggregateCreated(id),
                        new SecurityIdentityAdded(id, new UserId(15), "my ID", "openId", "generated-32")
                    },
                When = new RemoveSecurityItem(id, user),
                Expect = {new SecurityItemRemoved(id, user, "openId", "identity")}
            };
    }
}