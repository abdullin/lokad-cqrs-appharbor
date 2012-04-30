#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

namespace Lokad.Cqrs.TapeStorage
{
    /// <summary>
    /// Factory for storing blocks of data into append-only storage,
    /// that is easily to scale and replicate. This is the foundation
    /// for event sourcing.
    /// </summary>
    public interface ITapeContainer
    {
        /// <summary>
        /// Gets or creates a new named stream.
        /// </summary>
        /// <param name="name">The name of the stream.</param>
        /// <returns>new stream instance</returns>
        ITapeStream GetOrCreateStream(string name);

        /// <summary>
        /// Initializes this storage for writing
        /// </summary>
        void InitializeForWriting();
    }
}