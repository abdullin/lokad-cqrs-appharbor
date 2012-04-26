using System;

namespace SaaS.Web.Models
{
    public sealed class RegisterWaitModel
    {
        public Guid RegistrationId { get; set; }

        public string Email { get; set; }
        public string Password { get; set; }
        public string CompanyName { get; set; }
        public string ContactPhone { get; set; }
        public string Problem { get; set; }
        public string RealName { get; set; }
    }
}