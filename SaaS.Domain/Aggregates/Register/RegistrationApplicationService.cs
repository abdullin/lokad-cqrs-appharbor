using System;
using Sample;
using Sample.Aggregates.Register;

namespace Hub.ApplicationServices.Registration
{
    public sealed class RegistrationApplicationService : 
        IRegistrationApplicationService, IApplicationService
    {
        readonly IEventStore _eventStore;
        
        readonly IDomainIdentityService _ids;
        readonly IUserIndexService _uniqueness;
        
        readonly PasswordGenerator _generator;
        public RegistrationApplicationService( IEventStore eventStore,IDomainIdentityService ids, IUserIndexService uniqueness,  PasswordGenerator generator)
        {
            _eventStore = eventStore;
            _ids = ids;
            _uniqueness = uniqueness;
            _generator = generator;
        }

        public void When(CreateRegistration c)
        {
            Update(c, aggregate => aggregate.CreateRegistration(c.Id,c.Info, _ids, _uniqueness,_generator));
        }

        void Update(ICommand<RegistrationId> c, Action<RegistrationAggregate> action)
        {
            var stream = _eventStore.LoadEventStream(c.Id);
            var state = new RegistrationState(stream.Events);
            var agg = new RegistrationAggregate(state);

            using (var capture = Context.CaptureForThread())
            {
                action(agg);
                _eventStore.AppendToStream(c.Id, stream.Version, agg.Changes, capture.Log);
            }
        }

        public void When(AttachUserToRegistration c)
        {
            Update(c, aggregate => aggregate.AttachUserToRegistration(c.UserId, c.UserDisplay, c.Token));
        }

        public void Execute(object command)
        {
            RedirectToWhen.InvokeCommand(this, command);
        }
    }
}