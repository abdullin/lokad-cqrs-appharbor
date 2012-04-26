#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

// ReSharper disable InconsistentNaming

namespace SaaS.Web
{
    public class MvcApplication : HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            //filters.Add(new RequireSslAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new {controller = "account", action = "index", id = UrlParameter.Optional} // Parameter defaults
                );
        }


        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            // we check if cookie is valid and update the identity
            // otherwise unauthenticated identity is used
            //Global.Forms.InitializeRequest();
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            GlobalState.InitializeSessionFromRequest();
        }
    }
}