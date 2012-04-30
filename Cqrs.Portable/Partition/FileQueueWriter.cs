#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.IO;
using System.Threading;

namespace Lokad.Cqrs.Partition
{
    public sealed class FileQueueWriter : IQueueWriter
    {
        readonly DirectoryInfo _folder;

        public string Name { get; private set; }
        public readonly string Suffix;

        public FileQueueWriter(DirectoryInfo folder, string name)
        {
            _folder = folder;
            Name = name;
            Suffix = Guid.NewGuid().ToString().Substring(0, 4);
        }

        static long UniversalCounter;

        public void PutMessage(byte[] envelope)
        {
            var id = Interlocked.Increment(ref UniversalCounter);
            var fileName = string.Format("{0:yyyy-MM-dd-HH-mm-ss}-{1:00000000}-{2}", DateTime.UtcNow, id, Suffix);
            var full = Path.Combine(_folder.FullName, fileName);
            File.WriteAllBytes(full, envelope);
        }
    }
}