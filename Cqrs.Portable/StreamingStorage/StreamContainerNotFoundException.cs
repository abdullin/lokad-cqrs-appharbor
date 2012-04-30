#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Runtime.Serialization;

namespace SaaS.StreamingStorage
{
    [Serializable]
    public class StreamContainerNotFoundException : StreamBaseException
    {
        public StreamContainerNotFoundException(string message, Exception inner)
            : base(message, inner) {}

        protected StreamContainerNotFoundException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context) {}
    }
}