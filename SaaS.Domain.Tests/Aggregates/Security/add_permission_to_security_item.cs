#region (c) 2010-2012 Lokad - CQRS Sample for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using Sample;

namespace SaaS.Aggregates.Security
{
    public sealed class add_permission_to_security_item : specs
    {
        public static readonly SecurityId id = new SecurityId(42);
        public static readonly UserId user = new UserId(15);

        public spec when_valid_item = new security_spec
            {
                Given =
                    {
                        new SecurityAggregateCreated(id),
                        new SecurityIdentityAdded(id, new UserId(15), "my key", "legacy-key", "generated-32")
                    },
                When = new AddPermissionToSecurityItem(id, user, "root"),
                Expect = {new PermissionAddedToSecurityItem(id, user, "my key", "root", "generated-32")}
            };

        public spec when_duplicate_permission = new security_spec
            {
                Given =
                    {
                        new SecurityAggregateCreated(id),
                        new SecurityIdentityAdded(id, new UserId(15), "my key", "legacy-key", "generated-32"),
                        new PermissionAddedToSecurityItem(id, user, "my key", "root", "generated-32")
                    },
                When = new AddPermissionToSecurityItem(id, user, "root"),
            };

        public spec when_nonexistent_item = new security_fail
            {
                Given =
                    {
                        new SecurityAggregateCreated(id),
                    },
                When = new AddPermissionToSecurityItem(id, user, "root"),
                Expect = {error => error.Name == "invalid-user"}
            };
    }
}