using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Lokad.Cqrs.AtomicStorage;

namespace SaaS.Client.Projections.Registration
{
    [DataContract]
    public sealed class RegistrationView
    {
        [DataMember(Order = 1)]
        public string Status { get; set; }
        [DataMember(Order = 2)]
        public bool HasProblems { get; set; }
        [DataMember(Order = 3)]
        public bool Completed { get; set; }
        [DataMember(Order = 4)]
        public UserId UserId { get; set; }
        [DataMember(Order = 5)]
        public SecurityId SecurityId { get; set; }
        [DataMember(Order = 6)]
        public string UserDisplayName { get; set; }
        [DataMember(Order = 7)]
        public string UserToken { get; set; }
        [DataMember(Order = 8)]
        public string Problem { get; set; }
        [DataMember(Order = 9)]
        public string[] Permissions { get; set; }
        [DataMember(Order = 10)]
        public RegistrationId Registration { get; set; }

        public RegistrationView()
        {
            Permissions = new string[0];
        }

    }

    public sealed class RegistrationsProjection
    {
        readonly IDocumentWriter<RegistrationId, RegistrationView> _entity;
        public RegistrationsProjection(IDocumentWriter<RegistrationId, RegistrationView> entity)
        {
            _entity = entity;
        }

        public void When(RegistrationCreated e)
        {
            _entity.Add(e.Id, new RegistrationView
            {
                Status = "Processing registration",
                Registration = e.Id
            });
        }

        public void When(RegistrationSucceeded e)
        {
            _entity.UpdateOrThrow(e.Id, v =>
            {
                v.HasProblems = false;
                v.Completed = true;
                v.Status = "Registration completed";
                v.UserId = e.UserId;
                v.UserDisplayName = e.UserDisplayName;
                v.UserToken = e.UserToken;
                v.SecurityId = e.SecurityId;
            });
        }

        public void When(RegistrationFailed e)
        {
            _entity.Add(e.Id, new RegistrationView()
            {
                HasProblems = true,
                Status = "Problem discovered",
                Completed = true,
                Problem = string.Join(Environment.NewLine, e.Problems)
            });
        }
    }


}
