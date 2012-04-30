#region (c) 2010-2012 Lokad - CQRS Sample for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using Sample;

namespace SaaS.Aggregates.Security
{
    public sealed class security_spec : AggregateSpecification<SecurityId>
    {
        public IUserIndexService Index = new TestUserIndexService();
        public IDomainIdentityService Identity = new TestIdentityService();
        public TestPassword Passwords = new TestPassword();

        public security_spec()
        {
            Docs.Add(() => Index);
            Docs.Add(() => Identity);
            Docs.Add(() => Passwords);


            Handler = () => new SecurityApplicationService(Factory, Identity, Passwords, Index);
        }
    }

    public sealed class security_fail : AggregateFailSpecification<SecurityId, DomainError>
    {
        public IUserIndexService Index = new TestUserIndexService();
        public IDomainIdentityService Identity = new TestIdentityService();
        public TestPassword Passwords = new TestPassword();

        public security_fail()
        {
            Docs.Add(() => Index);
            Docs.Add(() => Identity);
            Docs.Add(() => Passwords);

            Handler = () => new SecurityApplicationService(Factory, Identity, Passwords, Index);
        }
    }
}