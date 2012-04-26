#region (c) 2010-2012 Lokad - CQRS Sample for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

namespace Sample.Aggregates.Security
{
    public class add_security_identity : specs
    {
        static readonly SecurityId id = new SecurityId(42);
        static readonly UserId user = new UserId(15);

        public spec add_identity = new security_spec
            {
                Identity = TestIdentityService.start_from(15),
                Given = {new SecurityAggregateCreated(id),},
                When = new AddSecurityIdentity(id, "my ID", "openID"),
                Expect = {new SecurityIdentityAdded(id, user, "my ID", "openID", "generated-32")}
            };
    }
}