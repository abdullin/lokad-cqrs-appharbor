#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Lokad.Cqrs.StreamingStorage
{
    /// <summary>
    /// Storage container using <see cref="System.IO"/> for persisting data
    /// </summary>
    public sealed class FileStreamContainer : IStreamContainer, IStreamRoot
    {
        readonly DirectoryInfo _root;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileStreamContainer"/> class.
        /// </summary>
        /// <param name="root">The root.</param>
        public FileStreamContainer(DirectoryInfo root)
        {
            _root = root;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileStreamContainer"/> class.
        /// </summary>
        /// <param name="path">The path.</param>
        public FileStreamContainer(string path) : this(new DirectoryInfo(path)) {}

        public IStreamContainer GetContainer(string name)
        {
            var child = new DirectoryInfo(Path.Combine(_root.FullName, name));
            return new FileStreamContainer(child);
        }

        public IStreamItem GetItem(string name)
        {
            var file = new FileInfo(Path.Combine(_root.FullName, name));
            return new FileStreamItem(file);
        }

        public IStreamContainer Create()
        {
            _root.Create();
            return this;
        }

        public void Delete()
        {
            _root.Refresh();
            if (_root.Exists)
                _root.Delete(true);
        }

        public bool Exists()
        {
            _root.Refresh();
            return _root.Exists;
        }

        public IEnumerable<string> ListItems()
        {
            try
            {
                return _root.GetFiles().Select(f => f.Name).ToArray();
            }
            catch (DirectoryNotFoundException e)
            {
                throw StreamErrors.ContainerNotFound(this, e);
            }
        }

        public string FullPath
        {
            get { return _root.FullName; }
        }
    }
}