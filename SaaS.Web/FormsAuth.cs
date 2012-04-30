#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Security.Principal;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.Security;
using Lokad.Cqrs.AtomicStorage;
using SaaS.AtomicStorage;
using SaaS.Client;
using SaaS.Client.Projections.LoginView;

namespace SaaS.Web
{
    /// <summary>
    /// Primary authentication-related functionality is aggregated here.
    /// Do not spread it over the solution, since we might need to replace it later!
    /// </summary>
    public class FormsAuth
    {
        readonly IDocumentReader<UserId, LoginView> _view;

        static readonly IPrincipal Anonymous = new GenericPrincipal(new GenericIdentity(""), new string[0]);


        public FormsAuth(IDocumentReader<UserId, LoginView> view)
        {
            _view = view;
        }

        public void Logout()
        {
            GlobalState.Clear();

            FormsAuthentication.SignOut();

            var context = HttpContext.Current;

            context.Session.Abandon();
        }

        /// <summary>
        /// Performs the HTTP Request initialization
        /// </summary>
        public void InitializeRequest()
        {
            // we check if cookie is valid and update the identity
            // otherwise unauthenticated identity is used

            var context = HttpContext.Current;

            if (!context.Request.IsAuthenticated)
                return;

            var formsPrincipal = context.User;
            // that should be a cookie string
            var username = formsPrincipal.Identity.Name;

            GetOrLoadRealPrincipal(username)
                .Apply(p => context.User = p)
                .Handle(() => context.User = Anonymous);
        }


        Maybe<AuthPrincipal> GetOrLoadRealPrincipal(string username)
        {
            var cache = HttpContext.Current.Cache;

            var cacheKey = "GlobalAuth_" + username;
            var principal = cache[cacheKey] as Maybe<AuthPrincipal>;

            if (principal == null)
            {
                // abdullin: this is not the global resolution cache
                // (that one is located down in the gateway logic)
                // but rather a small lookup one
                principal = LoadPrincipalInner(username);

                cache.Insert(
                    cacheKey,
                    principal,
                    null,
                    DateTime.UtcNow.AddMinutes(10),
                    Cache.NoSlidingExpiration);

                cache[cacheKey] = principal;
            }
            return principal;
        }

        Maybe<SessionIdentity> VerifyAndLoad(AuthInfo info)
        {
            var maybe = _view.Get(info.Login);
            if (!maybe.HasValue)
                return Maybe<SessionIdentity>.Empty;
            var view = maybe.Value;
            // Stored token does not match actual token.
            // Crash this one
            if (!string.Equals(view.Token, info.Token))
                return Maybe<SessionIdentity>.Empty;
            return new SessionIdentity(info.Login, view.Security, view.Display, info.ToCookieString(), view.Permissions,
                info.Token);
        }

        Maybe<AuthPrincipal> LoadPrincipalInner(string cookieString)
        {
            try
            {
                return AuthInfo.Parse(cookieString)
                    .Combine(VerifyAndLoad)
                    .Convert(si => new AuthPrincipal(si));
            }
            catch (Exception)
            {
                return Maybe<AuthPrincipal>.Empty;
            }
        }


        /// <summary>
        /// Gets the account associated with the current request.
        /// </summary>
        /// <returns>account that is associated with the current request</returns>
        public static Maybe<SessionIdentity> GetSessionIdentityFromRequest()
        {
            var principal = HttpContext.Current.User as AuthPrincipal;
            if (null == principal)
                return Maybe<SessionIdentity>.Empty;
            return principal.Identity;
        }


        public ActionResult HandleLogin(SessionIdentity account, bool persistCookie, string redirect = null)
        {
            return HandleLogin(account, persistCookie,
                () => { HttpContext.Current.Session["LoginAge"] = DateTime.UtcNow; }, redirect);
        }

        public bool IsLoginRecent()
        {
            var date = HttpContext.Current.Session["LoginAge"];
            // expired
            if (null == date)
                return false;

            try
            {
                return ((DateTime.UtcNow - (DateTime) date) < TimeSpan.FromSeconds(15));
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }
        }

        public ActionResult HandleLogin(SessionIdentity account, bool persistCookie, Action onSessionInit, string url)
        {
            GlobalState.InitializeSession(account);

            onSessionInit();
            FormsAuthentication.SetAuthCookie(account.CookieString, persistCookie);

            if (string.IsNullOrEmpty(url))
            {
                url = FormsAuthentication.GetRedirectUrl(account.CookieString, persistCookie);
                // this one crashes dev fabric
                //FormsAuthentication.RedirectFromLoginPage(username, persistCookie);
            }
            // we might need to unencode
            while (url.StartsWith("%"))
            {
                url = Uri.UnescapeDataString(url);
            }
            // this is MVC app, so no default.aspx any more
            if (url.EndsWith("/default.aspx"))
            {
                url = null;
            }

            if (string.IsNullOrEmpty(url))
            {
                // web site might be located in the nested folder
                url = HttpContext.Current.Request.ApplicationPath;
            }

            return new RedirectResult(url, false);
        }
    }
}