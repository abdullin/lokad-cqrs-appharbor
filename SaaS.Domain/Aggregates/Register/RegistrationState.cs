#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Collections.Generic;

namespace Sample.Aggregates.Register
{
    public sealed class RegistrationState : IRegistrationState
    {
        public RegistrationId Id { get; private set; }


        public RegistrationState(IEnumerable<IEvent<IIdentity>> events)
        {
            Problems = new List<string>();
            foreach (var e in events)
            {
                Mutate(e);
            }
        }

        public IList<string> Problems { get; private set; }

        public bool Success { get; set; }
        public CustomerId Customer { get; private set; }
        public SecurityId Security { get; private set; }
        public User FirstUser { get; private set; }

        public CustomerInfo CustomerInfo { get; private set; }
        public SecurityInfo SecurityInfo { get; private set; }

        public bool CanComplete
        {
            get { return !Success && Problems.Count == 0; }
        }

        public void When(RegistrationFailed c)
        {
            foreach (var problem in c.Problems)
            {
                Problems.Add(problem);
            }
        }

        public void When(RegistrationSucceeded e)
        {
            Success = true;
        }

        
        public void When(UserAttachedToRegistration e)
        {
            if (null == FirstUser)
            {
                FirstUser = new User(e.UserId, e.UserDisplay, e.Token);
            }
        }

        public void When(RegistrationCreated e)
        {
            Id = e.Id;
            Customer = e.Customer.CustomerId;
            Security = e.Security.SecurityId;
            CustomerInfo = e.Customer;
            SecurityInfo = e.Security;
        }

        public sealed class User
        {
            public readonly UserId Id;
            public readonly string Display;
            public readonly string Token;

            public User(UserId id, string display, string token)
            {
                Id = id;
                Display = display;
                Token = token;
            }
        }


        public void Mutate(IEvent<IIdentity> e)
        {
            RedirectToWhen.InvokeEventOptional(this, e);
        }
    }
}