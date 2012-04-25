using System.Collections.Generic;
using Lokad.Cqrs;
using Lokad.Cqrs.AtomicStorage;
using Sample;

namespace SaaS.Client
{
    public static class ClientBoundedContext
    {
        public static IEnumerable<object> Projections(IDocumentStore store)
        {
            yield return new LoginViewProjection(store.GetWriter<UserId, LoginView>());
            yield return new LoginsIndexProjection(store.GetWriter<unit, LoginsIndexView>());
        }
    }
}