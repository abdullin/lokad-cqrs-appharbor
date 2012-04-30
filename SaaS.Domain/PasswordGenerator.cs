#region (c) 2010-2012 Lokad - CQRS Sample for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Security.Cryptography;
using System.Text;

namespace SaaS
{
    /// <summary>
    /// Class that makes login authentication compatible with ASP.NET SqlMembership.
    /// </summary>
    public class PasswordGenerator
    {
        static readonly RNGCryptoServiceProvider Provider = new RNGCryptoServiceProvider();
        static readonly HashAlgorithm PasswordHasher = HashAlgorithm.Create("SHA1");


        /// <summary>
        /// Creates the random password, using strong random provider
        /// </summary>
        /// <returns>strong random password</returns>
        public virtual string CreatePassword(int length)
        {
            var buffer = new byte[length];
            Provider.GetBytes(buffer);
            return Convert.ToBase64String(buffer);
        }

        public virtual string CreateToken()
        {
            return CreatePassword(32);
        }

        public virtual string CreateSalt()
        {
            var buffer = new byte[16];
            Provider.GetBytes(buffer);
            return Convert.ToBase64String(buffer);
        }

        /// <summary>
        /// Hashes the password with the methods that are compatible with ASP.NET SqlMembership.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <param name="passwordSalt">The password salt.</param>
        /// <returns>password hash that could be saved to the database.</returns>
        public virtual string HashPassword(string password, string passwordSalt)
        {
            var saltBytes = Convert.FromBase64String(passwordSalt);
            var passwordBytes = Encoding.Unicode.GetBytes(password);
            var bytesToHash = new byte[saltBytes.Length + passwordBytes.Length];
            Buffer.BlockCopy(saltBytes, 0, bytesToHash, 0, saltBytes.Length);
            Buffer.BlockCopy(passwordBytes, 0, bytesToHash, saltBytes.Length, passwordBytes.Length);

            var inArray = PasswordHasher.ComputeHash(bytesToHash);
            return Convert.ToBase64String(inArray);
        }
    }
}