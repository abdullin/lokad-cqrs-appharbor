using System.Collections.Generic;
using System.Runtime.Serialization;
using Lokad.Cqrs;
using Lokad.Cqrs.AtomicStorage;

namespace SaaS.Client.Projections.Releases
{
    [DataContract]
    public sealed class RegisteredUsers
    {
        [DataMember(Order = 1)]
        public List<User> Items { get; set; }

        public RegisteredUsers()
        {
            Items = new List<User>();
        }
    }
      [DataContract]
    public sealed class User
    {
          [DataMember(Order = 1)]
          public string Login { get; set; }
    }


    public class RegisteredProjection
    {
        readonly IDocumentWriter<unit, RegisteredUsers> _writer;
        public RegisteredProjection(IDocumentWriter<unit, RegisteredUsers> writer)
        {
            _writer = writer;
        }

        public void When(SecurityPasswordAdded e)
        {
            _writer.UpdateEnforcingNew(unit.it, view => view.Items.Add(new User()
                {
                    Login = e.Login
                }));
        }
    }
}