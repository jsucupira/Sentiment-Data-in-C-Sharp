using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Data.Contracts;
using Timer = System.Timers.Timer;

namespace TwitterService
{
    public static class RetrieveData
    {
        private static readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private static readonly CancellationToken _cancellationToken = _cts.Token;

        public static void GettingDataFromTwitter(IStorage storage)
        {
            string checkTimer = ConfigurationManager.AppSettings["stop_after"];
            if (!String.IsNullOrEmpty(checkTimer))
            {
                int timeout;
                if (Int32.TryParse(checkTimer, out timeout))
                {
                    double totalMileseconds = (timeout*60)*1000;
                    Timer timer = new Timer(totalMileseconds);
                    timer.Elapsed += timer_Elapsed;
                    timer.Start();
                }
            }

            TwitterStream twitterStream = new TwitterStream(storage);

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

        private static void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _cts.Cancel();
        }
    }
}