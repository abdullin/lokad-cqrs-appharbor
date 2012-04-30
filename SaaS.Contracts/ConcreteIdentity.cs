#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;

namespace SaaS
{
    public static class IdentityConvert
    {
        public static string ToStream(IIdentity identity)
        {
            return identity.GetTag() + "-" + identity.GetId();
        }

        public static string ToTransportable(IIdentity identity)
        {
            return identity.GetTag() + "-" + identity.GetId();
        }
    }


    [DataContract(Namespace = "Sample")]
    public sealed class SecurityId : AbstractIdentity<long>
    {
        public const string TagValue = "security";

        public SecurityId(long id)
        {
            Contract.Requires(id > 0);
            Id = id;
        }

        public override string GetTag()
        {
            return TagValue;
        }


        [DataMember(Order = 1)]
        public override long Id { get; protected set; }

        public SecurityId() {}
    }


    [DataContract(Namespace = "Sample")]
    public sealed class UserId : AbstractIdentity<long>
    {
        public const string TagValue = "user";

        public UserId(long id)
        {
            if (id <= 0)
                throw new InvalidOperationException("Tried to assemble non-existent login");
            Id = id;
        }

        public override string GetTag()
        {
            return TagValue;
        }

        public UserId() {}

        [DataMember(Order = 1)]
        public override long Id { get; protected set; }
    }

    [DataContract(Namespace = "Sample")]
    public sealed class RegistrationId : AbstractIdentity<Guid>
    {
        public const string TagValue = "reg";

        public RegistrationId(Guid id)
        {
            Id = id;
        }

        public override string GetTag()
        {
            return TagValue;
        }

        public RegistrationId() {}

        [DataMember(Order = 1)]
        public override Guid Id { get; protected set; }

        public override string ToString()
        {
            return string.Format("reg-" + Id.ToString().ToLowerInvariant().Substring(0, 6));
        }
    }


    [DataContract(Namespace = "Sample")]
    public sealed class CustomerId : AbstractIdentity<long>
    {
        public const string TagValue = "user";

        public CustomerId(long id)
        {
            if (id <= 0)
                throw new InvalidOperationException("Tried to assemble non-existent login");
            Id = id;
        }

        public override string GetTag()
        {
            return TagValue;
        }

        public CustomerId() {}

        [DataMember(Order = 1)]
        public override long Id { get; protected set; }
    }


}