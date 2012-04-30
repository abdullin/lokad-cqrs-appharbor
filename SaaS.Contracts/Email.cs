#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Runtime.Serialization;

namespace SaaS
{
    [DataContract(Namespace = "Sample")]
    public class Email
    {
        [DataMember(Order = 1)]
        public string OptionalName { get; private set; }

        [DataMember(Order = 2)]
        public string Address { get; private set; }

        Email() {}

        public Email(string address, string optionalName = null)
        {
            OptionalName = optionalName;
            Address = address;
        }

        public bool Equals(Email other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.OptionalName, OptionalName) && Equals(other.Address, Address);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(Email)) return false;
            return Equals((Email) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((OptionalName != null ? OptionalName.GetHashCode() : 0) * 397) ^
                    (Address != null ? Address.GetHashCode() : 0);
            }
        }

        public static bool operator ==(Email x, Email y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(Email x, Email y)
        {
            return !(x == y);
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(OptionalName))
            {
                return string.Format("<{0}>", Address);
            }
            return string.Format("\"{0}\" <{1}>", OptionalName, Address);
        }
    }
}