using System.ComponentModel.Composition.Hosting;
using Core.DependencyResolver;
using Data.AzureBlob;
using Data.Contracts;
using Data.FileSystem;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TwitterService;

namespace StorageTest
{
    [TestClass]
    public class AzureBlobTest
    {

        [TestMethod]
        public void test_mef_resolver()
        {
            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(AzureBlobStorage).Assembly));
            ServiceLocator.Initialize(catalog);
            TwitterStream twitterStream = new TwitterStream(ServiceLocator.GetInstance<IStorage>());
            twitterStream.StreamData();
        }
    }
}