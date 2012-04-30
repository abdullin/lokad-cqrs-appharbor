#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

namespace SaaS.StreamingStorage
{
    /// <summary>
    /// Storage root (Azure Blob account or file drive)
    /// </summary>
    public interface IStreamRoot
    {
        /// <summary>
        /// Gets the container reference, identified by it's name
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>new container referece</returns>
        IStreamContainer GetContainer(string name);
    }
}