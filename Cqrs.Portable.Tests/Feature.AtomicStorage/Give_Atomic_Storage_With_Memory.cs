using Lokad.Cqrs.AtomicStorage;
using NUnit.Framework;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    [TestFixture]
    public sealed class Give_Atomic_Storage_With_Memory : Given_Atomic_Storage
    {
        protected override NuclearStorage Compose(IDocumentStrategy strategy)
        {
            return new MemoryStorageConfig().CreateNuclear(strategy);
        }
    }
}