#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;

namespace SaaS.Envelope.Events
{
    /// <summary>
    /// Raised when something goes wrong with the envelope deserialization (i.e.: unknown format or contract)
    /// </summary>
    [Serializable]
    public sealed class EnvelopeDeserializationFailed : ISystemEvent
    {
        public Exception Exception { get; private set; }
        public string Origin { get; private set; }

        public EnvelopeDeserializationFailed(Exception exception, string origin)
        {
            Exception = exception;
            Origin = origin;
        }


        public override string ToString()
        {
            return string.Format("Failed to deserialize in '{0}': '{1}'", Origin, Exception.Message);
        }
    }
}