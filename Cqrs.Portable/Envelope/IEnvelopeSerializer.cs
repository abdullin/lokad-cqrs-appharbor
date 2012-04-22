#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.IO;

namespace Lokad.Cqrs.Envelope
{
    public interface IEnvelopeSerializer
    {
        void SerializeEnvelope(Stream stream, EnvelopeContract c);
        EnvelopeContract DeserializeEnvelope(Stream stream);
    }
}