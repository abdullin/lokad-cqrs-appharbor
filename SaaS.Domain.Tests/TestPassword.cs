#region (c) 2010-2012 Lokad - CQRS Sample for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

namespace SaaS
{
    public sealed class TestPassword : PasswordGenerator
    {
        public override string CreatePassword(int length)
        {
            return "generated-" + length;
        }

        public override string CreateSalt()
        {
            return "salt";
        }

        public override string HashPassword(string password, string passwordSalt)
        {
            return password + "+" + passwordSalt;
        }

        public const string Token = "generated-32";
    }
}