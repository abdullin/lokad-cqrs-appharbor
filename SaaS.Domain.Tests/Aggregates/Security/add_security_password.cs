#region (c) 2010-2012 Lokad - CQRS Sample for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using Sample;

namespace SaaS.Aggregates.Security
{
    public class add_security_password : specs
    {
        static readonly SecurityId id = new SecurityId(42);
        static readonly UserId user = new UserId(15);

        public spec given_aggregate = new security_spec
            {
                Identity = TestIdentityService.start_from(15),
                Given = {new SecurityAggregateCreated(id),},
                When = new AddSecurityPassword(id, "my user", "login", "pass"),
                Expect = {new SecurityPasswordAdded(id, user, "my user", "login", "pass+salt", "salt", "generated-32")}
            };
    }
}