#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.IO;

namespace SaaS
{
    public sealed class FileStorageConfig
    {
        public DirectoryInfo Folder { get; private set; }
        public string AccountName { get; private set; }

        public string FullPath
        {
            get { return Folder.FullName; }
        }

        public FileStorageConfig(DirectoryInfo folder, string accountName)
        {
            Folder = folder;
            AccountName = accountName;
        }

        public FileStorageConfig SubFolder(string path, string account = null)
        {
            return new FileStorageConfig(new DirectoryInfo(Path.Combine(Folder.FullName, path)),
                account ?? AccountName + "-" + path);
        }

        public void Wipe()
        {
            if (Folder.Exists)
                Folder.Delete(true);
        }

        public void EnsureDirectory()
        {
            Folder.Create();
        }

        public void Reset()
        {
            Wipe();
            EnsureDirectory();
        }
    }
}