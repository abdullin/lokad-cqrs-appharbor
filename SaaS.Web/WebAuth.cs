#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using SaaS.Client;
using Sample;

namespace SaaS.Web
{
    public sealed class WebAuth
    {
        internal static bool CheckPassword(string existingHash, string existingSalt, string password)
        {
            var base64Hash = HashPassword(password, existingSalt);
            return existingHash == base64Hash;
        }

        static string HashPassword(string password, string passwordSalt)
        {
            var saltBytes = Convert.FromBase64String(passwordSalt ?? "");
            var passwordBytes = Encoding.Unicode.GetBytes(password ?? "");
            var bytesToHash = new byte[saltBytes.Length + passwordBytes.Length];
            Buffer.BlockCopy(saltBytes, 0, bytesToHash, 0, saltBytes.Length);
            Buffer.BlockCopy(passwordBytes, 0, bytesToHash, saltBytes.Length, passwordBytes.Length);

            using (var passwordHasher = HashAlgorithm.Create("SHA1"))
            {
                var inArray = passwordHasher.ComputeHash(bytesToHash);
                return Convert.ToBase64String(inArray);
            }
        }

        readonly WebEndpoint _webEndpoint;

        public WebAuth(WebEndpoint webEndpoint)
        {
            _webEndpoint = webEndpoint;
        }

        public AuthenticationResult PerformIdentityAuth(string identity)
        {
            long userId;
            var logins = _webEndpoint.GetSingleton<LoginsIndexView>().Identities;
            if (!logins.TryGetValue(identity, out userId))
            {
                // login not found
                return AuthenticationResult.UnknownIdentity;
            }
            var id = new UserId(userId);

            var maybe = _webEndpoint.GetView<LoginView>(id);

            if (!maybe.HasValue)
            {
                // we haven't created view, yet
                return AuthenticationResult.UnknownIdentity;
            }


            var view = maybe.Value;

            // ok, the view is locked. 
            if (view.LockedOutTillUtc > DateTime.UtcNow.AddSeconds(2))
            {
                return new AuthenticationResult(ComposeLockoutMessage(view));
            }

            // direct conversion, will be updated to hashes

            ReportLoginSuccess(id, view);
            return ViewToResult(id, view);
        }

        void ReportLoginSuccess(UserId id, LoginView view)
        {
            // we are good. Now, report success, if needed, to update all the views
            var time = DateTime.UtcNow;
            string host = HttpContext.Current.Request.UserHostAddress;
            if ((view.LastLoginUtc == DateTime.MinValue) || ((time - view.LastLoginUtc) > view.LoginTrackingThreshold))
            {
                _webEndpoint.SendOne(new ReportUserLoginSuccess(id, time, host));
            }
        }

        public AuthenticationResult PerformKeyAuth(string key)
        {
            if (string.IsNullOrEmpty(key))
                return AuthenticationResult.UnknownKey;

            // keys are always trimmed
            key = key.Trim();

            long userId;
            var indexView = _webEndpoint.GetSingleton<LoginsIndexView>();

            if (!indexView.Keys.TryGetValue(key, out userId))
            {
                // key not found
                return AuthenticationResult.UnknownKey;
            }

            var id = new UserId(userId);

            var maybe = _webEndpoint.GetView<LoginView>(id);
            // we haven't created view, yet
            if (!maybe.HasValue)
                return AuthenticationResult.UnknownKey;

            var view = maybe.Value;
            // ok, the view is locked. Since this could be the attacker or valid user alike,
            // display lock-out message to everybody
            if (view.LockedOutTillUtc > DateTime.UtcNow.AddSeconds(2))
            {
                return new AuthenticationResult(ComposeLockoutMessage(view));
            }

            ReportLoginSuccess(id, view);
            return ViewToResult(id, view);
        }

        static string ComposeLockoutMessage(LoginView view)
        {
            var lockoutTill = view.LockedOutTillUtc;

            var message = view.LockoutMessage;
            var current = DateTime.UtcNow;
            if (lockoutTill.Year - current.Year > 1)
            {
                // this is a really long lockout
                return message;
            }
            var diff = lockoutTill - current;
            string timer;
            if (diff.TotalHours > 1)
            {
                timer = string.Format("Account locked out till {0:yyyy-MM-dd HH:mm}", lockoutTill);
            }
            else
            {
                timer = string.Format("Account locked out for {0} minutes", Math.Ceiling(diff.TotalMinutes));
            }

            if (!string.IsNullOrEmpty(message))
                timer += ". " + message;
            return timer;
        }

        public AuthenticationResult PerformLoginAuth(string login, string password)
        {
            const string unknownLogin = "Unknown username or invalid password.";
            long userId;
            var indexView = _webEndpoint.GetSingleton<LoginsIndexView>();

            if (!indexView.Logins.TryGetValue(login, out userId))
            {
                // login not found
                return new AuthenticationResult(unknownLogin);
            }
            var id = new UserId(userId);

            var maybe = _webEndpoint.GetView<LoginView>(id);
            // we haven't created view, yet
            if (!maybe.HasValue)
                return new AuthenticationResult(unknownLogin);
            ;

            var view = maybe.Value;
            // ok, the view is locked. Since this could be the attacker or valid user alike,
            // display lock-out message to everybody
            if (view.LockedOutTillUtc > DateTime.UtcNow.AddSeconds(2))
            {
                return new AuthenticationResult(ComposeLockoutMessage(view));
            }


            if (!CheckPassword(view.PasswordHash, view.PasswordSalt, password))
            {
                // oups.
                string host = HttpContext.Current.Request.UserHostAddress;
                _webEndpoint.SendOne(new ReportUserLoginFailure(id, DateTime.UtcNow, host));
                return new AuthenticationResult(unknownLogin);
            }

            ReportLoginSuccess(id, view);
            return ViewToResult(id, view);
        }

        static AuthenticationResult ViewToResult(UserId id, LoginView view)
        {
            var auth = new AuthInfo(id, view.Token);
            var result =
                new AuthenticationResult(new SessionIdentity(id, view.Security, view.Display, auth.ToCookieString(),
                    view.Permissions, view.Token));
            return result;
        }
    }
}