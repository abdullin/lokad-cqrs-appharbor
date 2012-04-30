#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.IO;

namespace Lokad.Cqrs.StreamingStorage
{
    /// <summary>
    /// File-based implementation of the <see cref="IStreamItem"/>
    /// </summary>
    public sealed class FileStreamItem : IStreamItem
    {
        readonly FileInfo _file;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileStreamItem"/> class.
        /// </summary>
        /// <param name="file">The file.</param>
        public FileStreamItem(FileInfo file)
        {
            _file = file;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileStreamItem"/> class.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public FileStreamItem(string filePath)
        {
            _file = new FileInfo(filePath);
        }

        public Stream OpenRead()
        {
            Refresh();

            ThrowIfContainerNotFound();
            ThrowIfItemNotFound();

            // we allow concurrent writing or reading
            return _file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public Stream OpenWrite()
        {
            Refresh();

            ThrowIfContainerNotFound();

            // we allow concurrent reading
            // no more writers are allowed
            return _file.Open(FileMode.Create, FileAccess.ReadWrite, FileShare.None);
        }

        public bool Exists()
        {
            Refresh();
            return _file.Exists;
        }

        void ThrowIfItemNotFound()
        {
            if (!_file.Exists)
                throw StreamErrors.ItemNotFound(this);
        }

        /// <summary>
        /// Removes the item, ensuring that the specified condition is met.
        /// </summary>
        public void Delete()
        {
            Refresh();

            ThrowIfContainerNotFound();

            if (_file.Exists)
                _file.Delete();
        }

        void Refresh()
        {
            _file.Refresh();
        }

        void ThrowIfContainerNotFound()
        {
            if (!_file.Directory.Exists)
                throw StreamErrors.ContainerNotFound(this);
        }

        /// <summary>
        /// Gets the full path of the current item.
        /// </summary>
        /// <value>The full path.</value>
        public string FullPath
        {
            get { return _file.FullName; }
        }

        /// <summary>
        /// Gets the file reference behind this instance.
        /// </summary>
        /// <value>The reference.</value>
        public FileInfo Reference
        {
            get { return _file; }
        }
    }
}