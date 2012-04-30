#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

namespace SaaS
{
    public sealed class ImmutableAttribute
    {
        public string Key { get; private set; }
        public string Value { get; private set; }

        public ImmutableAttribute(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}", Key, Value);
        }
    }
}