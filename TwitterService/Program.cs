using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Text;
using Core.DependencyResolver;
using Data.AzureBlob;
using Data.Contracts;
using Data.FileSystem;
using Domain;
using NDesk.Options;

namespace TwitterService
{
    internal static class Program
    {
        private static IStorage _storage;

        private static void Main(string[] args)
        {

            Console.WriteLine("Starting to process data");

            bool showHelp = false;
            bool streamData = false;
            bool nonsentiment = false;
            bool byState = false;
            bool readData = false;
            string processFile = null;
            string processStartDate = null;
            string processEndDate = null;
            string storageType = "FileSystem";

            OptionSet p = new OptionSet
            {
                {
                    "storage=", "type the storage type [OPTIONAL]. [FileSystem], [AzureStorage], or [MSMQ]", v => storageType = v
                },
                {
                    "s|stream=", "Stream twitter data. This is true or false [OPTIONAL]", (bool v) => streamData = v
                },
                {
                    "readdata=", "Read data saved in storage [OPTIONAL]", (bool v) => readData = v
                },
                {
                    "sentimentbystate=", "Analyse sentiment by US state [OPTIONAL]", (bool v) => byState = v
                },
                {
                    "nonsentiment=", "Analyse the twitter files for words found in the tweet and its calculated scores [OPTIONAL]", (bool v) => nonsentiment = v
                },
                {
                    "processEndDate=", "Process the data retrieved from twitter [OPTIONAL]. The value should be the end date for when the file was created", v => processEndDate = v
                },
                {
                    "filename=", "Process the data retrieved from twitter [OPTIONAL]. This value should be the file name that contains the tweets", v => processFile = v
                },
                {
                    "processStartDate=", "Process the data retrieved from twitter [OPTIONAL]. The value should be the start date for when the file was created", v => processStartDate = v
                },
                {
                    "h|help", "show this message and exit", v => showHelp = v != null
                },
            };

            if (ParseArguments(args, p, showHelp)) return;

            SetupStorage(storageType);


            if (streamData)
            {
                Console.WriteLine("type quit to stop the process");
                RetrieveData.GettingDataFromTwitter(_storage);
            }
            else if (byState)
            {
                AnalyseByState(processFile, processStartDate, processEndDate);
            }
            else if (nonsentiment)
            {
                AnalyseNonSentiments(processFile, processStartDate, processEndDate);
            }
            else if (!string.IsNullOrEmpty(processFile))
            {
                if (File.Exists("total_sentiment.txt"))
                    File.Delete("total_sentiment.txt");
                var sentiment = ParseTwitterData.RetrieveTweetsScores(_storage, processFile);

                File.AppendAllText("total_sentiment.txt", sentiment.ToString());
                Console.WriteLine("Total scores for the range requested is {0}", ParseTwitterData.RetrieveTweetsScores(_storage, processFile));
            }
            else if (!string.IsNullOrEmpty(processStartDate) && !string.IsNullOrEmpty(processEndDate))
            {
                if (File.Exists("total_sentiment.txt"))
                    File.Delete("total_sentiment.txt");
                var sentiment = ParseTwitterData.RetrieveTweetsScores(_storage, new[] { processStartDate, processEndDate });

                File.AppendAllText("total_sentiment.txt", sentiment.ToString());

                Console.WriteLine("Total scores for the range requested is {0}", ParseTwitterData.RetrieveTweetsScores(_storage, new[] { processStartDate, processEndDate }));
            }
            else if (!string.IsNullOrEmpty(processStartDate))
            {
                if (File.Exists("total_sentiment.txt"))
                    File.Delete("total_sentiment.txt");
                var sentiment = ParseTwitterData.RetrieveTweetsScores(_storage, processStartDate);

                File.AppendAllText("total_sentiment.txt", sentiment.ToString());

                Console.WriteLine("Total scores for the range requested is {0}", ParseTwitterData.RetrieveTweetsScores(_storage, processStartDate));
            }
            else if (readData)
            {
                if (File.Exists("total_sentiment.txt"))
                    File.Delete("total_sentiment.txt");
                var sentiment = ParseTwitterData.RetrieveTweetsScores(_storage, null);
                File.AppendAllText("total_sentiment.txt", sentiment.ToString());
                Console.WriteLine("Total scores for the range requested is {0}", sentiment);

            }

            Console.WriteLine("Finished...");
        }

        private static bool ParseArguments(string[] args, OptionSet p, bool showHelp)
        {
            List<string> extra;
            try
            {
                extra = p.Parse(args);
            }
            catch (OptionException e)
            {
                Error(p, e.Message);
                return true;
            }

            if (extra.Count > 0)
                Error(p, "Incorrect parameters");

            if (showHelp)
            {
                ShowHelp(p);
                return true;
            }
            return false;
        }

        private static void AnalyseNonSentiments(string processFile, string processStartDate, string processEndDate)
        {
            if (File.Exists("non_sentiment.txt"))
                File.Delete("non_sentiment.txt");

            StringBuilder nonSentiments = new StringBuilder();
            if (!string.IsNullOrEmpty(processFile))
            {
                var items = ParseTwitterData.RetrieveTermSentiments(_storage, processFile);
                foreach (var item in items.OrderBy(t => t.Value))
                    nonSentiments.AppendLine(string.Format("{0} = {1}", item.Key, item.Value));
            }
            else if (!string.IsNullOrEmpty(processStartDate) && !string.IsNullOrEmpty(processEndDate))
            {
                var items = ParseTwitterData.RetrieveTermSentiments(_storage, new[] { processStartDate, processEndDate });
                foreach (var item in items.OrderBy(t => t.Value))
                    nonSentiments.AppendLine(string.Format("{0} = {1}", item.Key, item.Value));
            }
            else if (!string.IsNullOrEmpty(processStartDate))
            {
                var items = ParseTwitterData.RetrieveTermSentiments(_storage, processStartDate);
                foreach (var item in items.OrderBy(t => t.Value))
                    nonSentiments.AppendLine(string.Format("{0} = {1}", item.Key, item.Value));
            }
            else
            {
                var items = ParseTwitterData.RetrieveTermSentiments(_storage, null);
                foreach (var item in items.OrderBy(t => t.Value))
                    nonSentiments.AppendLine(string.Format("{0} = {1}", item.Key, item.Value));
            }

            File.AppendAllText("non_sentiment.txt", nonSentiments.ToString());
            Console.WriteLine("Saved non_sentiment.txt created.");
        }

        private static void AnalyseByState(string processFile, string processStartDate, string processEndDate)
        {
            if (File.Exists("by_state.txt"))
                File.Delete("by_state.txt");
            StringBuilder stateSentiment = new StringBuilder();
            if (!string.IsNullOrEmpty(processFile))
            {
                var items = ParseTwitterData.RetrieveScoresByUSStates(_storage, processFile);
                foreach (var item in items.OrderBy(t => t.Key))
                    stateSentiment.AppendLine(string.Format("{0} = {1}", item.Key, item.Value));
            }
            else if (!string.IsNullOrEmpty(processStartDate) && !string.IsNullOrEmpty(processEndDate))
            {
                var items = ParseTwitterData.RetrieveScoresByUSStates(_storage, new[] { processStartDate, processEndDate });
                foreach (var item in items.OrderBy(t => t.Key))
                    stateSentiment.AppendLine(string.Format("{0} = {1}", item.Key, item.Value));
            }
            else if (!string.IsNullOrEmpty(processStartDate))
            {
                var items = ParseTwitterData.RetrieveScoresByUSStates(_storage, processStartDate);
                foreach (var item in items.OrderBy(t => t.Key))
                    stateSentiment.AppendLine(string.Format("{0} = {1}", item.Key, item.Value));
            }
            else
            {
                var items = ParseTwitterData.RetrieveScoresByUSStates(_storage, null);
                foreach (var item in items.OrderBy(t => t.Key))
                    stateSentiment.AppendLine(string.Format("{0} = {1}", item.Key, item.Value));
            }

            File.AppendAllText("by_state.txt", stateSentiment.ToString());
            Console.WriteLine("Saved by_state.txt created.");
        }

        private static void SetupStorage(string storageType)
        {
            AggregateCatalog catalog = new AggregateCatalog();

            if (storageType.Equals("AzureStorage", StringComparison.OrdinalIgnoreCase))
                catalog.Catalogs.Add(new AssemblyCatalog(typeof(AzureBlobStorage).Assembly));
            else if (storageType.Equals("MSMQ", StringComparison.OrdinalIgnoreCase))
            {
                //catalog.Catalogs.Add(new AssemblyCatalog(typeof(MsmQueueStorage).Assembly));
                //testing out how to load just the dll. In case the implemementation was not in the actual solution
                catalog.Catalogs.Add(new DirectoryCatalog(@"..\..\..\lib\msmq\", "Data.MSQueue.dll"));
            }
            else
                catalog.Catalogs.Add(new AssemblyCatalog(typeof(FileSystemStorage).Assembly));

            ServiceLocator.Initialize(catalog);
            _storage = ServiceLocator.GetInstance<IStorage>();
        }

        private static void Error(OptionSet p, string message)
        {
            Console.WriteLine();
            Console.Write("TwitterService: " + message + "\r\n");
            Console.WriteLine();
            Console.WriteLine("Try `TwitterService --help' for more information.");
            p.WriteOptionDescriptions(Console.Out);
            Console.WriteLine();
        }

        private static void ShowHelp(OptionSet p)
        {
            Console.WriteLine();
            Console.WriteLine("Usage: TwitterService [OPTIONS]");
            Console.WriteLine();
            Console.WriteLine("Stream data from twitter or process the data retrieved from twitter.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
            Console.WriteLine();
            Console.WriteLine("Example: TwitterService -stream or TwitterService - process");
            Console.WriteLine();
        }
    }
}