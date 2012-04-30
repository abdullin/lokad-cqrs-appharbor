#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.IO;

namespace SaaS.AtomicStorage
{
    public sealed class FileDocumentStore : IDocumentStore
    {
        readonly string _folderPath;
        readonly IDocumentStrategy _strategy;

        public FileDocumentStore(string folderPath, IDocumentStrategy strategy)
        {
            _folderPath = folderPath;
            _strategy = strategy;
        }

        public override string ToString()
        {
            return new Uri(Path.GetFullPath(_folderPath)).AbsolutePath;
        }


        readonly HashSet<Tuple<Type, Type>> _initialized = new HashSet<Tuple<Type, Type>>();


        public IDocumentWriter<TKey, TEntity> GetWriter<TKey, TEntity>()
        {
            var container = new FileDocumentReaderWriter<TKey, TEntity>(_folderPath, _strategy);
            if (_initialized.Add(Tuple.Create(typeof(TKey), typeof(TEntity))))
            {
                container.InitIfNeeded();
            }
            return container;
        }

        public IDocumentReader<TKey, TEntity> GetReader<TKey, TEntity>()
        {
            return new FileDocumentReaderWriter<TKey, TEntity>(_folderPath, _strategy);
        }

        public IDocumentStrategy Strategy
        {
            get { return _strategy; }
        }


        public IEnumerable<DocumentRecord> EnumerateContents(string bucket)
        {
            var full = Path.Combine(_folderPath, bucket);
            var dir = new DirectoryInfo(full);
            if (!dir.Exists) yield break;

            var fullFolder = dir.FullName;
            foreach (var info in dir.EnumerateFiles("*", SearchOption.AllDirectories))
            {
                var fullName = info.FullName;
                var path = fullName.Remove(0, fullFolder.Length + 1).Replace(Path.DirectorySeparatorChar, '/');
                yield return new DocumentRecord(path, () => File.ReadAllBytes(fullName));
            }
        }

        public void WriteContents(string bucket, IEnumerable<DocumentRecord> records)
        {
            var buck = Path.Combine(_folderPath, bucket);
            if (!Directory.Exists(buck))
                Directory.CreateDirectory(buck);
            foreach (var pair in records)
            {
                var recordPath = Path.Combine(buck, pair.Key);

                var path = Path.GetDirectoryName(recordPath) ?? "";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                File.WriteAllBytes(recordPath, pair.Read());
            }
        }

        public void ResetAll()
        {
            if (Directory.Exists(_folderPath))
                Directory.Delete(_folderPath, true);
            Directory.CreateDirectory(_folderPath);
        }

        public void Reset(string bucket)
        {
            var path = Path.Combine(_folderPath, bucket);
            if (Directory.Exists(path))
                Directory.Delete(path, true);
            Directory.CreateDirectory(path);
        }
    }
}