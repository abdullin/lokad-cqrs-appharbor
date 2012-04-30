#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.IO;

namespace SaaS.StreamingStorage
{
    /// <summary>
    /// Base interface for performing storage operations against local or remote persistence.
    /// </summary>
    public interface IStreamItem
    {
        /// <summary>
        /// Gets the full path of the current iteб.
        /// </summary>
        /// <value>The full path.</value>
        string FullPath { get; }

        Stream OpenRead();
        Stream OpenWrite();

        bool Exists();

        /// <summary>
        /// Removes the item.
        /// </summary>
        void Delete();
    }
}