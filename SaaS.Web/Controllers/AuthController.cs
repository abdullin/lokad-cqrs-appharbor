#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Web.Mvc;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
using DotNetOpenAuth.OpenId.RelyingParty;

namespace SaaS.Web.Controllers
{
    public class AuthController : Controller
    {
        static readonly OpenIdRelyingParty Openid = new OpenIdRelyingParty();

        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("index", "account");
            }
            var returnUrl = Request.Params["ReturnUrl"];
            return RedirectToAction("login", new {returnUrl});
        }

        public ActionResult Logout()
        {
            Global.Forms.Logout();
            return RedirectToAction("index", "auth");
        }

        public ActionResult Login()
        {
            // Stage 1: display login form to user
            return View("login");
        }

        ActionResult LoginError(string message)
        {
            return View("login", (object) message);
        }

        [ValidateInput(false)]
        public ActionResult Authenticate(string returnUrl)
        {
            var response = Openid.GetResponse();
            if (response == null)
            {
                // Stage 2: user submitting Identifier
                Identifier id;
                var userSuppliedIdentifier = Request.Form["openid_identifier"] ?? Request.Form["openid_identifier_2"];

                if (string.IsNullOrEmpty(userSuppliedIdentifier))
                {
                    return LoginError("Please supply non-empty OpenID");
                }
                if (Identifier.TryParse(userSuppliedIdentifier, out id))
                {
                    try
                    {
                        var fetch = new FetchRequest();
                        fetch.Attributes.Add(new AttributeRequest(WellKnownAttributes.Contact.Email, true));

                        var request = Openid.CreateRequest(userSuppliedIdentifier);
                        request.AddExtension(fetch);

                        return request.RedirectingResponse.AsActionResult();
                    }
                    catch (ProtocolException ex)
                    {
                        return View("login", (object) ex.Message);
                    }
                }
                else
                {
                    return View("login", (object) "Invalid identifier");
                }
            }
            else
            {
                // Stage 3: OpenID Provider sending assertion response
                switch (response.Status)
                {
                    case AuthenticationStatus.Authenticated:
                        var ident = response.ClaimedIdentifier == null ? "null" : response.ClaimedIdentifier.ToString();
                        var result = Global.Auth.PerformIdentityAuth(ident);
                        if (result.IsSuccess)
                        {
                            return Global.Forms.HandleLogin(result.Identity, true);
                        }
                        else
                        {
                            return View("login", (object) result.ErrorMessage);
                        }


                    case AuthenticationStatus.Canceled:

                        return View("login", (object) "Canceled at provider");
                    case AuthenticationStatus.Failed:

                        return View("login", (object) response.Exception.Message);
                }
            }
            return new EmptyResult();
        }

        public ActionResult AuthLogin(string returnurl, string user, string password)
        {
            var result = (user == "key@lokad.com" || string.IsNullOrEmpty(user))
                ? Global.Auth.PerformKeyAuth(password)
                : Global.Auth.PerformLoginAuth(user, password);
            if (result.IsSuccess)
            {
                var permissions = result.Identity.Permissions;
                if (permissions.Contains("administrator"))
                {
                    return Global.Forms.HandleLogin(result.Identity, true, returnurl);
                }
                return View("login", (object) "Authorization has failed. Contact Lokad Admins");
            }
            return View("login", (object) result.ErrorMessage);
        }


        /// <summary>
        /// <code>GET: /auth/key/</code>
        /// <para>We try to login using the provided key</para>
        /// </summary>
        public ActionResult Key(string id)
        {
            if (Request.IsAuthenticated)
            {
                Global.Forms.Logout();
            }
            var result = Global.Auth.PerformKeyAuth(id.Trim());

            if (result.IsSuccess)
            {
                return Global.Forms.HandleLogin(result.Identity, true, string.Empty);
            }
            return View("login", (object) result.ErrorMessage);
        }
    }
}