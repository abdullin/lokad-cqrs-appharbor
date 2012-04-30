using Lokad.Cqrs.AtomicStorage;
using NUnit.Framework;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public abstract class Given_Atomic_Storage
    {
        protected abstract NuclearStorage Compose(IDocumentStrategy strategy);

        [Test]
        public void SimpleWriteRoundtrip()
        {
            var strategy = new TestStrategy();
            var setup = Compose(strategy);

            setup.AddOrUpdateEntity(1, "test");
            setup.AddOrUpdateSingleton(() => 1, i => 1);

            AssertContents(setup);

            var memStore = new MemoryStorageConfig();
            var mem = memStore.CreateNuclear(strategy);
            
            
            mem.CopyFrom(setup, strategy.GetEntityBucket<string>(), strategy.GetEntityBucket<int>());

            AssertContents(mem);

            setup.Container.Reset(strategy.GetEntityBucket<string>());
            setup.Container.Reset(strategy.GetEntityBucket<int>());

            setup.CopyFrom(mem, strategy.GetEntityBucket<string>(), strategy.GetEntityBucket<int>());

            AssertContents(setup);
        }

        private static void AssertContents(NuclearStorage setup)
        {
            Assert.AreEqual("test", setup.GetEntity<string>(1).Value);
            Assert.AreEqual(1, setup.GetSingleton<int>().Value);
        }
    }
}