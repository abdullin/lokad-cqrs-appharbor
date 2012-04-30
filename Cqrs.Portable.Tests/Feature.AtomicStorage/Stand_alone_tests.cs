using System;
using System.Runtime.Serialization;
using Lokad.Cqrs.AtomicStorage;
using NUnit.Framework;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    [TestFixture, Explicit]
    public sealed class Stand_alone_tests
    {
        // ReSharper disable InconsistentNaming
        [Test]
        public void Test()
        {
            
            var nuclearStorage = FileStorage.CreateConfig(GetType().Name).CreateNuclear(new TestStrategy());

            var writer = nuclearStorage.Container.GetWriter<unit,Dunno>();
            writer.UpdateEnforcingNew(unit.it, dunno => dunno.Count += 1);
            writer.UpdateEnforcingNew(unit.it, dunno => dunno.Count += 1);

            var count = nuclearStorage.Container.GetReader<unit,Dunno>().GetOrNew().Count;
            Console.WriteLine(count);
        }

        [DataContract]
        public sealed class Dunno
        {
            [DataMember]
            public int Count { get; set; }
        }
    }
}