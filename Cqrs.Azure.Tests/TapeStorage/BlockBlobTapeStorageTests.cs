using System;
using System.Diagnostics;
using System.Security.Cryptography;
using Lokad.Cqrs.Feature.TapeStorage;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using NUnit.Framework;

namespace Lokad.Cqrs.TapeStorage
{
    [TestFixture]
    class BlockBlobTapeStorageTests : TapeStorageTests
    {
        const string ContainerName = "blob-tape-test";

        readonly CloudStorageAccount _cloudStorageAccount = CloudStorageAccount.DevelopmentStorageAccount;
        ITapeContainer _storageFactory;


        [Test, Explicit]
        public void Performance_tests()
        {
            // random write.
            var gen = new RNGCryptoServiceProvider();
            var bytes = 1024;
            var data = new byte[bytes];
            gen.GetNonZeroBytes(data);
            var stopwatch = Stopwatch.StartNew();
            var records = 200;

            var total = records * bytes;
            Console.WriteLine("Data is {0} records of {1} bytes == {2} bytes or {3} MB", records, bytes, total, total / 1024 / 1024);

            for (int i = 0; i < records; i++)
            {
                _stream.TryAppend(data);
            }

            var timeSpan = stopwatch.Elapsed;
            Console.WriteLine("Writing one by one in {0}", timeSpan.TotalSeconds);

            int counter = 0;
            var reading = Stopwatch.StartNew();
            foreach (var tapeRecord in _stream.ReadRecords(0, int.MaxValue))
            {
                counter += tapeRecord.Data.Length;
            }
            Console.WriteLine("Reading in {0} seconds", reading.Elapsed.TotalSeconds);
            Console.WriteLine("Read {0} bytes of raw data", counter);

        }

        protected override void PrepareEnvironment()
        {
            var cloudBlobClient = _cloudStorageAccount.CreateCloudBlobClient();

            try
            {
                cloudBlobClient.GetContainerReference(ContainerName).FetchAttributes();
                throw new InvalidOperationException("Container '" + ContainerName + "' already exists!");
            }
            catch (StorageClientException e)
            {
                if (e.ErrorCode != StorageErrorCode.ResourceNotFound)
                    throw new InvalidOperationException("Container '" + ContainerName + "' already exists!");
            }
        }

        protected override ITapeStream InitializeAndGetTapeStorage()
        {
            var config = AzureStorage.CreateConfig(_cloudStorageAccount);
            _storageFactory = new BlockBlobTapeStorageFactory(config, ContainerName);
            _storageFactory.InitializeForWriting();

            const string name = "test";

            return _storageFactory.GetOrCreateStream(name);
        }

        protected override void FreeResources()
        {
            _storageFactory = null;
            _storageFactory = null;
        }

        protected override void TearDownEnvironment()
        {
            var cloudBlobClient = _cloudStorageAccount.CreateCloudBlobClient();
            var container = cloudBlobClient.GetContainerReference(ContainerName);

            if (container.Exists())
                container.Delete();
        }
    }
}
