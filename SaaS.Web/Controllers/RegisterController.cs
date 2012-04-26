using System;
using System.Collections.Specialized;
using System.Web.Mvc;
using SaaS.Client;
using SaaS.Client.Projections.Registration;
using SaaS.Web.Models;
using Sample;

namespace SaaS.Web.Controllers
{
    public sealed class RegisterController : Controller
    {
        public ActionResult Index()
        {
            return View("index");
        }

        [HttpPost]
        public ActionResult Index(RegisterModel model)
        {

            if (ModelState.IsValidField("Email"))
            {
                // check uniqueness on the server
                var index = Global.Client.GetSingleton<LoginsIndexView>();
                if (index.ContainsLogin(model.Email))
                {
                    // could this customer be already registered???
                    // people don't confuse their emails often
                    var result = Global.Auth.PerformLoginAuth(model.Email, model.Password);
                    // OK, this could be customer trying to re-register
                    // let's log him in
                    if (result.IsSuccess)
                    {
                        return Global.Forms.HandleLogin(result.Identity, false);
                    }
                    else
                    {
                        ModelState.AddModelError("Email", string.Format("Email {0} is taken", model.Email));
                    }
                }
            }

            if (!ModelState.IsValid)
                return View("index", model);



            var newGuid = Guid.NewGuid();

            var coll = new NameValueCollection(Request.Headers);

            if (!string.IsNullOrEmpty(Request.UserHostAddress))
            {
                coll.Add("UserHostAddress", Request.UserHostAddress);
            }
            foreach (var language in Request.UserLanguages ?? new string[0])
            {
                coll.Add("UserLanguages", language);
            }

            var reg = new RegistrationInfoBuilder(model.Email, model.CompanyName)
            {
                OptionalUserPassword = model.Password,
                OptionalCompanyPhone = model.ContactPhone,
                Headers = coll,
                OptionalUserName = model.RealName
            };
            Global.Client.SendOne(new CreateRegistration(new RegistrationId(newGuid), reg.Build()));

            return View("wait", new RegisterWaitModel
            {
                RegistrationId = newGuid,
                CompanyName = model.CompanyName,
                ContactPhone = model.ContactPhone,
                Email = model.Email,
                Password = model.Password,
                RealName = model.RealName
            });
        }

        public ActionResult Correct(RegisterModel model)
        {
            return View("index", model);
        }
        public ActionResult Finish(Guid id)
        {
            var view = Global.Client.GetView<RegistrationView>(new RegistrationId(id));
            var reg = view.Value;


            if (reg.Completed && !reg.HasProblems)
            {
                var log = SessionIdentity.Create(reg.UserDisplayName, reg.UserId, reg.UserToken, reg.SecurityId);
                // auto-login!
                return Global.Forms.HandleLogin(log, false, Url.Action("welcome", "account"));
            }

            throw new InvalidOperationException("Invalid reg");
        }
        public ActionResult CheckStatus(Guid id)
        {
            var view = Global.Client.GetView<RegistrationView>(new RegistrationId(id));
            var partialViewResult = PartialView("status", view.HasValue ? view.Value : null);

            return partialViewResult;
        }



    }
}