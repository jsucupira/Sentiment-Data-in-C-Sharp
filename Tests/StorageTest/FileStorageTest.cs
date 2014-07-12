using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using System.Threading;
using Core.DependencyResolver;
using Data.Contracts;
using Data.FileSystem;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TwitterService;

namespace StorageTest
{
    [TestClass]
    public class FileStorageTest
    {
        [TestMethod]
        public void test_tick_behavior()
        {
            List<long> ticks = new List<long>();

            for (int i = 0; i < 1000; i++)
            {
                ticks.Add(DateTime.UtcNow.Ticks);
                Thread.Sleep(TimeSpan.FromSeconds(2));
            }
        }

        [TestMethod]
        public void test_mef_resolver()
        {
            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(FileSystemStorage).Assembly));
            ServiceLocator.Initialize(catalog);

            TwitterStream twitterStream = new TwitterStream(ServiceLocator.GetInstance<IStorage>());
            twitterStream.StreamData();
        }
    }
}
