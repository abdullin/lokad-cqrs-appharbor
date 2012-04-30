#region (c) 2010-2012 Lokad - CQRS Sample for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Security.Cryptography;
using System.Text;
using Lokad.Cqrs.AtomicStorage;

namespace SaaS.Services.UserIndex
{
    public sealed class UserIndexProjection
    {
        readonly IDocumentWriter<byte, UserIndexLookup> _writer;

        public UserIndexProjection(IDocumentWriter<byte, UserIndexLookup> writer)
        {
            _writer = writer;
        }

        byte GetKey(string name)
        {
            using (MD5 md = new MD5CryptoServiceProvider())
            {
                var bytes = Encoding.UTF8.GetBytes(name.ToLowerInvariant());
                return md.ComputeHash(bytes)[0];
            }
        }

        public void When(SecurityPasswordAdded e)
        {
            var b = GetKey(e.Login);
            _writer.UpdateEnforcingNew(b, si => si.Logins[e.Login] = e.UserId.Id);
        }

        public void When(SecurityIdentityAdded e)
        {
            var b = GetKey(e.Identity);
            _writer.UpdateEnforcingNew(b, si => si.Identities[e.Identity] = e.UserId.Id);
        }

        public void When(SecurityItemRemoved e)
        {
            var b = GetKey(e.Lookup);
            _writer.UpdateEnforcingNew(b, si =>
                {
                    si.Logins.Remove(e.Lookup);
                    si.Identities.Remove(e.Lookup);
                });
        }
    }
}