#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Concurrent;
using System.IO;

namespace Lokad.Cqrs.TapeStorage
{
    public sealed class FileTapeContainer : ITapeContainer
    {
        readonly string _fullPath;

        readonly ConcurrentDictionary<string, ITapeStream> _writers =
            new ConcurrentDictionary<string, ITapeStream>();


        public FileTapeContainer(string fullPath)
        {
            _fullPath = fullPath;
        }

        public ITapeStream GetOrCreateStream(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (string.IsNullOrWhiteSpace("name"))
                throw new ArgumentException("Incorrect value.", "name");

            var writer = _writers.GetOrAdd(name, n => BuildTapStream(name));

            return writer;
        }

        ITapeStream BuildTapStream(string name)
        {
            if (!Directory.Exists(_fullPath))
            {
                throw new InvalidOperationException(string.Format("Root folder should exist: " + _fullPath));
            }
            // This is fast and allows to have git-style subfolders in atomic strategy
            // to avoid NTFS performance degradation (when there are more than 
            // 10000 files per folder). Kudos to Gabriel Schenker for pointing this out
            var combine = Path.Combine(_fullPath, name);
            var subfolder = Path.GetDirectoryName(combine) ?? "";
            if (subfolder != _fullPath && !Directory.Exists(subfolder))
            {
                Directory.CreateDirectory(subfolder);
            }

            return new FileTapeStream(combine);
        }

        public void InitializeForWriting()
        {
            Directory.CreateDirectory(_fullPath);
        }
    }
}