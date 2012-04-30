#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Runtime.Serialization;

namespace Lokad.Cqrs.StreamingStorage
{
    [Serializable]
    public class StreamItemNotFoundException : StreamBaseException
    {
        public StreamItemNotFoundException(string message, Exception inner) : base(message, inner) {}


        protected StreamItemNotFoundException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context) {}
    }
}