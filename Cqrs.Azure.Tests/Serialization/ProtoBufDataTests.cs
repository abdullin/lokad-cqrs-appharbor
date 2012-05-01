﻿#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Runtime.Serialization;
using Lokad.Cqrs.Evil;
using NUnit.Framework;

namespace Lokad.Cqrs.Serialization
{
    [TestFixture]
    public sealed class ProtoBufDataTests : ProtobufFixture
    {
        // ReSharper disable InconsistentNaming


        [Test]
        public void Roundtrip_Data()
        {
            var result = RoundTrip(new SimpleDataClass("Some"));
            Assert.AreEqual("Some", result.Field);
        }

        [Test, Ignore("Proto-buf currently does not support extensions for DCA")]
        public void Roundtrip_Legacy()
        {
            var result = RoundTrip(new SimpleDataClass("Some"), typeof (CustomDataClass));
            Assert.AreEqual("Some", result.Field);
        }


        [Test]
        public void Default_reference_is_type_name()
        {
            var contractReference = ContractEvil.GetContractReference(typeof (SimpleDataClass));
            Assert.AreEqual("ProtoBufDataTests/SimpleDataClass", contractReference);
        }

        [Test]
        public void Class_can_override()
        {
            var contractReference = ContractEvil.GetContractReference(typeof (CustomDataClass));
            Assert.AreEqual("Custom/ProtoBufDataTests/Type", contractReference);
        }

        [DataContract]
        public sealed class SimpleDataClass : IExtensibleDataObject
        {
            [DataMember(Order = 1)]
            public string Field { get; private set; }

            public SimpleDataClass(string field)
            {
                Field = field;
            }


            SimpleDataClass()
            {
            }

            public ExtensionDataObject ExtensionData { get; set; }
        }

        [DataContract(Namespace = "Custom", Name = "Type")]
        public sealed class CustomDataClass : IExtensibleDataObject
        {

            public ExtensionDataObject ExtensionData { get; set; }
        }
    }
}