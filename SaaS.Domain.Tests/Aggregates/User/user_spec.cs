#region (c) 2010-2012 Lokad - CQRS Sample for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

// ReSharper disable InconsistentNaming

using Sample;

namespace SaaS.Aggregates.User
{
    public sealed class user_spec : AggregateSpecification<UserId>
    {
        public user_spec()
        {
            Handler = () => new UserApplicationService(Factory);
        }
    }

    public sealed class user_fail : AggregateFailSpecification<UserId, DomainError>
    {
        public user_fail()
        {
            Handler = () => new UserApplicationService(Factory);
        }
    }
}