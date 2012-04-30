using Lokad.Cqrs.AtomicStorage;
using NUnit.Framework;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    [TestFixture]
    public sealed class Given_Atomic_Storage_With_Files : Given_Atomic_Storage
    {
        protected override NuclearStorage Compose(IDocumentStrategy strategy)
        {
            return _config.CreateNuclear(strategy);
        }

        public Given_Atomic_Storage_With_Files()
        {
            _config = FileStorage.CreateConfig(typeof(Given_Atomic_Scenarios_When_Files).Name);
        }

        readonly FileStorageConfig _config;
        

        [SetUp]
        public void SetUp()
        {
            _config.Wipe();
            _config.EnsureDirectory();
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            _config.Wipe();
        }
    }
}