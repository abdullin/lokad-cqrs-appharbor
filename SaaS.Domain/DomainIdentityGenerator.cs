#region (c) 2010-2012 Lokad - CQRS Sample for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Runtime.Serialization;
using Lokad.Cqrs.AtomicStorage;

namespace SaaS
{
    public sealed class DomainIdentityGenerator : IDomainIdentityService
    {
        readonly NuclearStorage _storage;

        public DomainIdentityGenerator(NuclearStorage storage)
        {
            _storage = storage;
        }

        public long GetId()
        {
            var ix = new long[1];
            _storage.UpdateSingletonEnforcingNew<DomainIdentityVector>(t => t.Reserve(ix));
            return ix[0];
        }

        public void IncrementDomainIdentity(long id)
        {
            _storage.UpdateSingletonEnforcingNew<DomainIdentityVector>(t =>
                {
                    if (t.EntityId < id)
                    {
                        t.EntityId = id;
                    }
                });
        }
    }

    [DataContract(Namespace = "hub-domain-data", Name = "domainidentityvector")]
    public sealed class DomainIdentityVector
    {
        [DataMember(Order = 1)]
        public long EntityId { get; set; }

        public void Reserve(long[] indexes)
        {
            for (int i = 0; i < indexes.Length; i++)
            {
                EntityId += 1;
                indexes[i] = EntityId;
            }
        }
    }
}