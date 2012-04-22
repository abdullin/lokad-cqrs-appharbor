#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Security.Principal;
using Sample;
using IIdentity = System.Security.Principal.IIdentity;

namespace SaaS.Web
{
    public sealed class SessionIdentity
    {
        public readonly UserId User;
        public readonly SecurityId Security;
        public readonly CustomerId Customer;

        public readonly string UserName;
        public readonly string SessionDisplay;
        public readonly string CookieString;
        public readonly HashSet<string> Permissions;
        public readonly string Token;


        public static SessionIdentity Create(string dispay, UserId user, string token, SecurityId sec,
            params string[] permissions)
        {
            var auth = new AuthInfo(user, token);
            return new SessionIdentity(user, sec, dispay, auth.ToCookieString(), permissions, token);
        }

        public SessionIdentity(
            UserId user,
            SecurityId customer,
            string userName,
            string cookieString,
            IEnumerable<string> permissions,
            string token)
        {
            User = user;
            Security = customer;
            UserName = userName;
            SessionDisplay = String.Format("{0} ({1})", UserName, customer.Id);

            CookieString = cookieString;
            Customer = new CustomerId(customer.Id);
            Permissions = new HashSet<string>(permissions);
            Token = token;
        }
    }

    public sealed class AuthenticationResult
    {
        public readonly string ErrorMessage;
        public readonly SessionIdentity Identity;
        public readonly bool IsSuccess;

        public static readonly AuthenticationResult UnknownIdentity = new AuthenticationResult("Unknown identity");
        public static readonly AuthenticationResult UnknownKey = new AuthenticationResult("Unknown key");

        public AuthenticationResult(string errorMessage)
        {
            ErrorMessage = errorMessage;
            IsSuccess = false;
        }

        public AuthenticationResult(SessionIdentity identity)
        {
            Identity = identity;
            IsSuccess = true;
        }
    }

    public sealed class AuthInfo
    {
        public readonly UserId Login;
        public readonly string Token;

        public AuthInfo(UserId login, string token)
        {
            Login = login;
            Token = token;
        }

        public static Maybe<AuthInfo> Parse(string cookieString)
        {
            if (string.IsNullOrEmpty(cookieString))
                return Maybe<AuthInfo>.Empty;
            if (!cookieString.StartsWith("v1|"))
                return Maybe<AuthInfo>.Empty;

            var strings = cookieString.Split('|');
            var login = long.Parse(strings[1]);
            var token = strings[2];
            return new AuthInfo(new UserId(login), token);
        }

        public string ToCookieString()
        {
            return string.Format("v1|{0}|{1}", Login.Id, Token);
        }
    }

    /// <summary>
    /// Implementation of <see cref="IPrincipal"/> that provides
    /// backward compatibility for legacy authorization rules and ELMAH.
    /// </summary>
    [Serializable]
    public sealed class AuthPrincipal : MarshalByRefObject, IPrincipal
    {
        /// <summary>
        /// Account associated with the principal
        /// </summary>
        public readonly SessionIdentity Identity;

        readonly IIdentity _identity;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthPrincipal"/> class.
        /// </summary>
        /// <param name="account">The account.</param>
        public AuthPrincipal(SessionIdentity account)
        {
            Identity = account;
            _identity = new GenericIdentity(account.SessionDisplay);
        }

        bool IPrincipal.IsInRole(string role)
        {
            return false;
        }

        IIdentity IPrincipal.Identity
        {
            get { return _identity; }
        }
    }
}