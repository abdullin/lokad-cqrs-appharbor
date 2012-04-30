#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;

namespace Lokad.Cqrs
{
    //[DebuggerDisplay("{MappedType.Name} with {")]
    public sealed class ImmutableMessage
    {
        public readonly Type MappedType;
        public readonly object Content;
        public readonly int Index;
        readonly ImmutableAttribute[] _attributes;

        public ICollection<ImmutableAttribute> GetAllAttributes()
        {
            return _attributes;
        }

        public bool TryGetAttribute(string name, out string result)
        {
            foreach (var attribute in _attributes)
            {
                if (attribute.Key == name)
                {
                    result = attribute.Value;
                    return true;
                }
            }
            result = null;
            return false;
        }

        public string GetAttribute(string name, string defaultValue)
        {
            foreach (var attribute in _attributes)
            {
                if (attribute.Key == name)
                    return attribute.Value;
            }
            return defaultValue;
        }


        public ImmutableMessage(Type mappedType, object content, ImmutableAttribute[] attributes, int index)
        {
            MappedType = mappedType;
            Index = index;
            Content = content;
            _attributes = attributes;
        }

        public override string ToString()
        {
            return string.Format("[{0}]", Content);
        }
    }
}