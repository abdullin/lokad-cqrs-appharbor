﻿#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.IO;
using Lokad.Cqrs.Feature.StreamingStorage.Scenarios;
using Lokad.Cqrs.StreamingStorage;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
namespace Lokad.Cqrs.Feature.StreamingStorage
{

    public sealed class Run_all_StreamingStorage_scenarios_for_Files : ITestStorage
    {
        readonly DirectoryInfo _root;

        public Run_all_StreamingStorage_scenarios_for_Files()
        {
            var root = Path.Combine(Path.GetTempPath(), "file-storage-tests");
            _root = new DirectoryInfo(root);
            _root.Create();
        }

        public IStreamContainer GetContainer(string name)
        {
            var combine = Path.Combine(_root.FullName, "test");
            return new FileStreamContainer(new DirectoryInfo(combine));
        }

        

        [TestFixture]
        public sealed class When_deleting_blob_item :
            When_deleting_item_in<Run_all_StreamingStorage_scenarios_for_Files>
        {
        }

        [TestFixture]
        public sealed class When_reading_blob_item :
            When_reading_item_in<Run_all_StreamingStorage_scenarios_for_Files>
        {
        }

        [TestFixture]
        public sealed class When_listing_blob_items :
            When_listing_items_in<Run_all_StreamingStorage_scenarios_for_Files>
        {
            
        }


        [TestFixture]
        public sealed class When_writing_blob_item
            : When_writing_item_in<Run_all_StreamingStorage_scenarios_for_Files>
        {
        }

        [TestFixture]
        public sealed class When_copying_blob_item
            : When_copying_items_in<Run_all_StreamingStorage_scenarios_for_Files>
        {
        }

        [TestFixture]
        public sealed class When_checking_blob_item
            : When_checking_item_in<Run_all_StreamingStorage_scenarios_for_Files>
        {
        }
    }
}