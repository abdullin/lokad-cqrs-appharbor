#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Collections.Generic;
using System.Linq;
using Sample;
using Sample.Aggregates.Register;

namespace Hub.ApplicationServices.Registration
{
    public sealed class RegistrationAggregate 
    {
        readonly RegistrationState _state;
        public IList<IEvent<IIdentity>> Changes = new List<IEvent<IIdentity>>();

        public RegistrationAggregate(RegistrationState state)
        {
            _state = state;
        }
        public void CreateRegistration(
            RegistrationId i, 
            RegistrationInfo info, 
            IDomainIdentityService ids,
            IUserIndexService uniqueness, 
            PasswordGenerator generator)
        {
            var problems = new List<string>();
            // we do all the checks at registration phase 
            if (uniqueness.IsLoginRegistered(info.ContactEmail))
            {
                problems.Add(string.Format("Email '{0}' is already taken.", info.ContactEmail));
            }

            if (!string.IsNullOrEmpty(info.OptionalUserIdentity))
            {
                if (uniqueness.IsIdentityRegistered(info.OptionalUserIdentity))
                {
                    problems.Add(string.Format("Identity '{0}' is already taken.", info.OptionalUserIdentity));
                }
            }

            var userDisplay = info.OptionalUserDisplay;
            if (string.IsNullOrEmpty(userDisplay))
            {
                userDisplay = string.Format("{0}", info.CustomerName);
            }

            var password = info.OptionalUserPassword;
            if (string.IsNullOrEmpty(password))
            {
                password = generator.CreatePassword(6);
            }
            // TODO: we are checking contact uniqueness, but can use user name
            var login = info.ContactEmail;
            if (string.IsNullOrEmpty(login))
            {
                login = info.ContactEmail;
            }


            
            if (problems.Any())
            {
                Apply(new RegistrationFailed(i, info, problems.ToArray()));
                return;
            }
            var id = ids.GetId();

            var host = info.Headers.FirstOrDefault(h => h.Key == "UserHostAddress");
            

            var security = new SecurityInfo(new SecurityId(id), login, password, userDisplay, info.OptionalUserIdentity);
            var customer = new CustomerInfo(new CustomerId(id), info.CustomerName, userDisplay, info.ContactEmail,
                info.OptionalCompanyPhone, info.OptionalCompanyUrl);

            Apply(new RegistrationCreated(i, info.CreatedUtc, customer, security));
            // if no problems
        }


        

        public void AttachUserToRegistration(UserId userId, string userDisplay, string token)
        {
            Apply(new UserAttachedToRegistration(_state.Id, userId, userDisplay, token));
            TryCompleteRegistration();
        }

        void TryCompleteRegistration()
        {
            if (!_state.CanComplete) return;


            var user = _state.FirstUser;
            Apply(new RegistrationSucceeded(_state.Id, _state.Customer, _state.Security, user.Id, user.Display, user.Token));
        }

        

        void Apply(IEvent<RegistrationId> e)
        {
            _state.Mutate(e);
            Changes.Add(e);
        }
    }
}