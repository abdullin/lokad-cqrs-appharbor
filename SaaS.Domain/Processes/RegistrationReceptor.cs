namespace SaaS.Processes
{
    public sealed class RegistrationReceptor
    {
        readonly DomainSender _flow;

        public RegistrationReceptor(DomainSender flow)
        {
            _flow = flow;
        }

        public void When(RegistrationCreated x)
        {
            // forwards token to customer
            var c = x.Customer;
            var s = x.Security;

            //_flow.ToCustomer(new CreateCustomerFromRegistration(
            //    c.CustomerId,
            //    x.Id,
            //    c.CompanyName,
            //    c.CustomerEmail,
            //    c.OptionalPhone,
            //    c.OptionalUrl,
            //    x.RegisteredUtc, c.CurrencyType, c.RealName));

            // and to security passport
            _flow.ToSecurity(new CreateSecurityFromRegistration(s.SecurityId, x.Id, s.Login, s.Pwd, s.UserDisplay,
                s.OptionalIdentity));
            // TODO add timeout tracker
        }

        public void When(SecurityRegistrationProcessCompleted e)
        {
            // sec => reg (completes registration)
            _flow.ToRegistration(new AttachUserToRegistration(e.RegistrationId, e.UserId, e.DisplayName, e.Token));
        }
    }
}