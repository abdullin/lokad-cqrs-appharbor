using System.Collections.Generic;
using Lokad.Cqrs;
using Lokad.Cqrs.AtomicStorage;
using SaaS.Client.Projections.Registration;
using SaaS.Client.Projections.Releases;
using Sample;

namespace SaaS.Client
{
    public static class ClientBoundedContext
    {
        public static IEnumerable<object> Projections(IDocumentStore docs)
        {
            yield return new LoginViewProjection(docs.GetWriter<UserId, LoginView>());
            yield return new LoginsIndexProjection(docs.GetWriter<unit, LoginsIndexView>());
            yield return new RegistrationsProjection(docs.GetWriter<RegistrationId, RegistrationView>());
            // system
            yield return new ReleasesProjection(docs.GetWriter<unit, ReleasesView>());
            yield return new RegisteredProjection(docs.GetWriter<unit, RegisteredUsers>());
        }
    }
}