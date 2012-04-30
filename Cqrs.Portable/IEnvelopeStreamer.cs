#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using SaaS.Envelope;

namespace SaaS
{
    /// <summary>
    /// Is responsible for reading-writing message envelopes either as
    /// data or references to data (in case envelope does not fit media)
    /// </summary>
    public interface IEnvelopeStreamer
    {
        /// <summary>
        /// Saves message envelope as array of bytes.
        /// </summary>
        /// <param name="envelope">The message envelope.</param>
        /// <returns></returns>
        byte[] SaveEnvelopeData(ImmutableEnvelope envelope);

        /// <summary>
        /// Reads the buffer as message envelope
        /// </summary>
        /// <param name="buffer">The buffer to read.</param>
        /// <returns>mes    sage envelope</returns>
        ImmutableEnvelope ReadAsEnvelopeData(byte[] buffer);
    }

    public static class ExtendIEnvelopeStreamer
    {
        public static byte[] SaveEnvelopeData(this IEnvelopeStreamer streamer, object message,
            Action<EnvelopeBuilder> build = null)
        {
            var builder = new EnvelopeBuilder(Guid.NewGuid().ToString());
            builder.AddItem(message);
            if (null != build)
            {
                build(builder);
            }
            return streamer.SaveEnvelopeData(builder.Build());
        }
    }
}