#region (c) 2010-2012 Lokad - CQRS Sample for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Security.Cryptography;
using System.Text;
using Lokad.Cqrs.AtomicStorage;

namespace SaaS.Services.UserIndex
{
    public sealed class UserIndexService : IUserIndexService
    {
        readonly NuclearStorage _storage;

        public UserIndexService(NuclearStorage storage)
        {
            _storage = storage;
        }

        public bool IsLoginRegistered(string email)
        {
            var key = GetKey(email);
            return _storage
                .GetEntity<UserIndexLookup>(key)
                .Convert(i => i.Logins.ContainsKey(email), false);
        }

        byte GetKey(string name)
        {
            using (MD5 md = new MD5CryptoServiceProvider())
            {
                var bytes = Encoding.UTF8.GetBytes(name.ToLowerInvariant());
                return md.ComputeHash(bytes)[0];
            }
        }

        public bool IsIdentityRegistered(string identity)
        {
            var key = GetKey(identity);
            return _storage
                .GetEntity<UserIndexLookup>(key)
                .Convert(i => i.Identities.ContainsKey(identity), false);
        }
    }
}