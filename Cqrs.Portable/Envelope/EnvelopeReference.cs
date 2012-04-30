#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

namespace SaaS.Envelope
{
    public sealed class EnvelopeReference
    {
        public readonly string StorageReference;
        public readonly string StorageContainer;

        public EnvelopeReference(string storageContainer, string storageReference)
        {
            StorageReference = storageReference;
            StorageContainer = storageContainer;
        }
    }
}