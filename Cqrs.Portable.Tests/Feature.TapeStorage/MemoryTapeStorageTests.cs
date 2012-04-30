using System.Collections.Concurrent;
using System.Collections.Generic;
using Lokad.Cqrs.TapeStorage;
using NUnit.Framework;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    [TestFixture]
    public sealed class MemoryTapeStorageTests : TapeStorageTests
    {
        // ReSharper disable InconsistentNaming

        
        MemoryTapeContainer _storageFactory = new MemoryTapeContainer();

        protected override void PrepareEnvironment()
        {
            _storageFactory = new MemoryTapeContainer();
        }

        protected override ITapeStream InitializeAndGetTapeStorage()
        {
            const string name = "Memory";
            return _storageFactory.GetOrCreateStream(name);
        }

        protected override void FreeResources()
        {
            //_storageFactory = null;
        }

        protected override void TearDownEnvironment()
        {
            _storageFactory = null;
        }
    }
}