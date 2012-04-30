#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;

namespace SaaS.Envelope
{
    /// <summary>
    /// Implements quarantine logic for the specific message handler. Default implementation 
    /// is <see cref="MemoryQuarantine"/>
    /// </summary>
    public interface IEnvelopeQuarantine
    {
        /// <summary>
        /// Tries to quarantine the specified envelope. Implementation can decide whether we need to give another
        /// try to process the envelope (by returning <em>False</em>) or if quarantine should accept the envelope
        /// completely. Then processor will discard the queue from it's incoming queue and leave it up to the
        /// quarantine to record it, push to poison etc. 
        /// </summary>
        /// <param name="context">The envelope transport context.</param>
        /// <param name="ex">The exception.</param>
        /// <returns><em>True</em> if envelope should be quarantined right away (i.e. exception happened 4 times)
        /// and is not excepted to be processed by the queue any more; <em>False</em> otherwise</returns>
        bool TryToQuarantine(ImmutableEnvelope optionalEnvelope, Exception ex);


        void Quarantine(byte[] message, Exception ex);

        /// <summary>
        /// Tries to release envelope record from the partial or full quarantine (I.e.: when message
        /// has been successfully processed and quarantine can forget about it forever).
        /// </summary>
        /// <param name="context">The envelope transport context.</param>
        void TryRelease(ImmutableEnvelope context);
    }
}