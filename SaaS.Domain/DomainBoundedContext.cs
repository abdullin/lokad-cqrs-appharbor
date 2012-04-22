#region (c) 2010-2012 Lokad - CQRS Sample for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lokad.Cqrs.AtomicStorage;
using Sample.Aggregates.Security;
using Sample.Aggregates.User;
using Sample.Processes;
using Sample.Services.UserIndex;

namespace Sample
{
    public static class DomainBoundedContext
    {
        public static string EsContainer = "hub-domain-tape";

        public static IEnumerable<object> Projections(IDocumentStore docs)
        {
            yield return new UserIndexProjection(docs.GetWriter<byte, UserIndexLookup>());
        }

        public static IEnumerable<Func<CancellationToken, Task>> Tasks(ICommandSender service, IDocumentStore docs,
            bool isTest)
        {
            var flow = new DomainSender(service);
            // more tasks go here
            yield break;
        }


        public static IEnumerable<object> Receptors(ICommandSender service)
        {
            var flow = new DomainSender(service);
            yield return new ReplicationReceptor(flow);
            // more senders go here
        }

        public static IEnumerable<object> ApplicationServices(IDocumentStore docs, IEventStore store)
        {
            var storage = new NuclearStorage(docs);
            var id = new DomainIdentityGenerator(storage);
            var unique = new UserIndexService(storage);
            var passwords = new PasswordGenerator();


            yield return new UserApplicationService(store);
            yield return new SecurityApplicationService(store, id, passwords, unique);

            yield return id;
        }
    }
}