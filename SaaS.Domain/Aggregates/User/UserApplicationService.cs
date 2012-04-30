#region (c) 2010-2012 Lokad - CQRS Sample for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;

namespace SaaS.Aggregates.User
{
    public sealed class UserApplicationService : IUserApplicationService, IApplicationService
    {
        readonly IEventStore _store;

        public UserApplicationService(IEventStore store)
        {
            _store = store;
        }

        void Update(ICommand<UserId> c, Action<UserAggregate> action)
        {
            var stream = _store.LoadEventStream(c.Id);
            var state = new UserState(stream.Events);
            var agg = new UserAggregate(state);

            using (var capture = Context.CaptureForThread())
            {
                agg.ThrowOnInvalidStateTransition(c);
                action(agg);
                _store.AppendToStream(c.Id, stream.Version, agg.Changes, capture.Log);
            }
        }

        public void When(CreateUser c)
        {
            Update(c, ar => ar.Create(c.Id, c.SecurityId));
        }

        public void When(ReportUserLoginFailure c)
        {
            Update(c, ar => ar.ReportLoginFailure(c.TimeUtc, c.Ip));
        }

        public void When(ReportUserLoginSuccess c)
        {
            Update(c, ar => ar.ReportLoginSuccess(c.TimeUtc, c.Ip));
        }

        public void When(LockUser c)
        {
            Update(c, ar => ar.Lock(c.LockReason));
        }

        public void When(UnlockUser c)
        {
            Update(c, user => user.Unlock(c.UnlockReason));
        }

        public void When(DeleteUser c)
        {
            Update(c, ar => ar.Delete());
        }

        public void Execute(object command)
        {
            RedirectToWhen.InvokeCommand(this, command);
        }
    }
}