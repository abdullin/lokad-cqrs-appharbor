#region (c) 2010-2012 Lokad - CQRS Sample for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

namespace Sample.Processes
{
    /// <summary>
    /// Replicates changes between <see cref="ISecurityAggregate"/> and 
    /// individual instances of <see cref="IUserAggregate"/>
    /// </summary>
    public sealed class ReplicationReceptor
    {
        // 'Domain' is the name of the primary Bounded Context
        // in this system
        readonly DomainSender _send;

        public ReplicationReceptor(DomainSender send)
        {
            _send = send;
        }

        public void When(SecurityPasswordAdded e)
        {
            _send.ToUser(new CreateUser(e.UserId, e.Id));
        }

        public void When(SecurityIdentityAdded e)
        {
            _send.ToUser(new CreateUser(e.UserId, e.Id));
        }


        public void When(SecurityItemRemoved e)
        {
            _send.ToUser(new DeleteUser(e.UserId));
        }
    }

    public sealed class DomainSender
    {
        readonly ICommandSender _service;

        public DomainSender(ICommandSender service)
        {
            _service = service;
        }

        public void ToRegistration(ICommand<RegistrationId> cmd)
        {
            _service.SendCommandsAsBatch(new ISampleCommand[] {cmd});
        }

        public void ToUser(ICommand<UserId> cmd)
        {
            _service.SendCommandsAsBatch(new ISampleCommand[] {cmd});
        }

        public void ToSecurity(ICommand<SecurityId> cmd)
        {
            _service.SendCommandsAsBatch(new ISampleCommand[] {cmd});
        }

        public void ToService(ISampleCommand cmd)
        {
            _service.SendCommandsAsBatch(new[] {cmd});
        }
    }
}