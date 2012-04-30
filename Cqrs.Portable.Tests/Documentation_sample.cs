#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Lokad.Cqrs.AtomicStorage;
using Lokad.Cqrs.Build;
using Lokad.Cqrs.Core;
using Lokad.Cqrs.Core.Envelope;
using Lokad.Cqrs.Envelope;
using Lokad.Cqrs.Feature.AtomicStorage;
using NUnit.Framework;

// ReSharper disable InconsistentNaming

namespace Lokad.Cqrs
{
    [TestFixture]
    public sealed class Documentation_sample
    {
 
        [DataContract]
        public sealed class CreateCustomer
        {
            [DataMember] public string CustomerName;
            [DataMember] public int CustomerId;
        }


        [DataContract]
        public sealed class CustomerCreated 
        {
            [DataMember] public int CustomerId;
            [DataMember] public string CustomerName;
        }

        [DataContract]
        public sealed class Customer
        {
            [DataMember] public string Name;
            [DataMember] public int Id;

            public Customer(int id, string name)
            {
                Name = name;
                Id = id;
            }
        }

        [Test, Explicit]
        public void Run_in_test()
        {
            var streamer = EnvelopeStreamer.CreateDefault(typeof(CreateCustomer), typeof(CustomerCreated));
            
            var builder = new CqrsEngineBuilder(streamer);
            var account = new MemoryStorageConfig();

            var nuclear = account.CreateNuclear(new TestStrategy());
            var inbox = account.CreateInbox("input");
            var sender = account.CreateSimpleSender(streamer, "input");



            var handler = new RedirectToCommand();
            handler.WireToLambda<CreateCustomer>(customer => Consume(customer, nuclear, sender));
            handler.WireToLambda<CustomerCreated>(m => Console.WriteLine("Created!"));
            builder.Handle(inbox,  envelope => handler.InvokeMany(envelope.SelectContents()));

            using (var engine = builder.Build())
            {
                sender.SendOne(new CreateCustomer
                {
                    CustomerId = 1,
                    CustomerName = "Rinat Abdullin"
                });
                engine.RunForever();
            }
        }


        static void Consume(CreateCustomer cmd, NuclearStorage storage, SimpleMessageSender sender)
        {
            var customer = new Customer(cmd.CustomerId, cmd.CustomerName);
            storage.AddEntity(customer.Id, customer);
            sender.SendOne(new CustomerCreated
                {
                    CustomerId = cmd.CustomerId,
                    CustomerName = cmd.CustomerName
                });
        }
    }
}