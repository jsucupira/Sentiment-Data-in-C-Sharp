using System;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Core.DependencyResolver;
using Data.AzureBlob;
using Data.Contracts;
using Data.FileSystem;

namespace TwitterService
{
    internal static class Program
    {
        static readonly CancellationTokenSource _cts = new CancellationTokenSource();
        static readonly CancellationToken _cancellationToken = _cts.Token;
        private static IStorage _storage;
        private static void Main(string[] args)
        {
            // Creating catagolo for dependencies
            AggregateCatalog catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(FileSystemStorage).Assembly));
            //catalog.Catalogs.Add(new AssemblyCatalog(typeof(AzureBlobStorage).Assembly));
            ServiceLocator.Initialize(catalog);
            _storage = ServiceLocator.GetInstance<IStorage>();

            Console.WriteLine("Starting to process data");
            Console.WriteLine("type quit to stop the process");

            //GettingDataFromTwitter();
            CheckingSavedFiles();
        }

        private static void CheckingSavedFiles()
        {
            //var getAllTweets = _storage.Get("635407453946620513.json");
            //var getAllTweets = _storage.Get("7/12/2014 3:02:57 AM");
            var getAllTweets = _storage.Get(635407453946620513);

            foreach (var tweet in getAllTweets)
            {
                if (!string.IsNullOrEmpty(tweet))
                { }
            }
        }

        private static void GettingDataFromTwitter()
        {
            var checkTimer = ConfigurationManager.AppSettings["stop_after"];
            if (!string.IsNullOrEmpty(checkTimer))
            {
                int timeout;
                if (int.TryParse(checkTimer, out timeout))
                {
                    double totalMileseconds = (timeout * 60) * 1000;
                    System.Timers.Timer timer = new System.Timers.Timer(totalMileseconds);
                    timer.Elapsed += timer_Elapsed;
                    timer.Start();
                }
            }

            TwitterStream twitterStream = new TwitterStream(_storage);

            Task.Factory.StartNew(() =>
            {
                try
                {
                    twitterStream.StreamData(_cancellationToken);
                }
                catch (AggregateException aggregateException)
                {
                    Console.WriteLine(aggregateException.Flatten().InnerExceptions);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }, _cancellationToken);

            while (Console.ReadLine() != "quit" && !_cancellationToken.IsCancellationRequested)
            {
            }

            if (!_cancellationToken.IsCancellationRequested && _cancellationToken.CanBeCanceled)
                _cts.Cancel();

            Thread.Sleep(TimeSpan.FromSeconds(10));
        }

        static void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _cts.Cancel();
        }
    }
}