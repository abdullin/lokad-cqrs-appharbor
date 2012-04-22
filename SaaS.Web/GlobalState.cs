#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Diagnostics;
using System.Web;
using Sample;

namespace SaaS.Web
{
    /// <summary>
    /// Static class that provides strongly-typed access to the session (user)-
    /// specific objects and variables. This includes session-specific container
    /// </summary>
    public static class GlobalState
    {
        const string AccountSessionKey = "GlobalSetup_ASK";

        public static void Clear()
        {
            var session = HttpContext.Current.Session;
            session.Clear();
        }

        /// <summary>
        /// Single point of entry to initialize the session
        /// </summary>
        /// <param name="accountSession">The account session.</param>
        public static void InitializeSession(SessionIdentity accountSession)
        {
            var session = HttpContext.Current.Session;
            session[AccountSessionKey] = accountSession;
        }

        /// <summary>
        /// Initializes the session, using the auth information
        /// associated with the current request
        /// </summary>
        public static void InitializeSessionFromRequest()
        {
            var context = HttpContext.Current;
            // we are fine
            if (context.Session[AccountSessionKey] != null)
                return;

            // unauthenticated session here
            if (!context.Request.IsAuthenticated)
                return;

            // authenticated session but without our data.
            // recover expired session (or use cookie)

            Debug.WriteLine("Session initialization attempt");
            FormsAuth
                .GetSessionIdentityFromRequest()
                .Apply(InitializeSession);
        }

        /// <summary>
        /// Gets a value indicating whether this instance is authenticated.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is authenticated; otherwise, <c>false</c>.
        /// </value>
        public static bool IsAuthenticated
        {
            get { return HttpContext.Current.Session[AccountSessionKey] != null; }
        }

        /// <summary>
        /// Gets the account associated with the current session.
        /// </summary>
        /// <value>The account associated with the current session.</value>
        public static SessionIdentity Identity
        {
            get
            {
                // session recovery is handled by the global handler
                return (SessionIdentity) HttpContext.Current.Session[AccountSessionKey];
            }
        }

        public static CustomerId Customer
        {
            get { return Identity.Customer; }
        }

        public static SecurityId Security
        {
            get { return Identity.Security; }
        }
    }
}