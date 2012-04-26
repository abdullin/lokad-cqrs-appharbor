#region (c) 2010-2012 Lokad - CQRS Sample for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

namespace Sample.Aggregates
{
    public sealed class TestIdentityService : IDomainIdentityService
    {
        long _initialId;

        public static IDomainIdentityService start_from(long id)
        {
            return new TestIdentityService
                {
                    _initialId = id,
                    _identity = id
                };
        }


        long _identity;

        public long GetId()
        {
            var id = _identity;
            _identity += 1;
            return id;
        }

        public override string ToString()
        {
            if (_identity != 0)
                return string.Format("Domain numbers start at {0}", _initialId);
            return null;
        }
    }
}