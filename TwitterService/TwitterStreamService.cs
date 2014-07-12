//code taken from https://github.com/swhitley/TwitterStreamClient
//GenerateTimeStamp was modified as well some naming convention

using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Core.Common.Authentication;
using Data.Contracts;

namespace TwitterService
{
    public class TwitterStream : OAuthBase
    {
        private readonly string _accessToken = ConfigurationManager.AppSettings["access_token"];
        private readonly string _accessTokenSecret = ConfigurationManager.AppSettings["access_token_secret"];
        private readonly string _customerKey = ConfigurationManager.AppSettings["customer_key"];
        private readonly string _customerSecret = ConfigurationManager.AppSettings["customer_secret"];
        private readonly int _maxTwitterCount = int.Parse(ConfigurationManager.AppSettings["twitter_message_size"]);

        private readonly IStorage _storage;
        private string _httpMethod = "POST";

        public TwitterStream(IStorage storage)
        {
            _storage = storage;
        }

        public void StreamData(CancellationToken cancellationToken = default (CancellationToken))
        {
            //Twitter Streaming API
            string streamUrl = ConfigurationManager.AppSettings["stream_url"];
            if (streamUrl == "https://stream.twitter.com/1/statuses/sample.json")
                _httpMethod = "GET";

            HttpWebRequest webRequest = null;
            HttpWebResponse webResponse = null;
            StreamReader responseStream = null;
            string postparameters = (ConfigurationManager.AppSettings["track_keywords"].Length == 0 ? string.Empty : "&track=" + ConfigurationManager.AppSettings["track_keywords"]) +
                                    (ConfigurationManager.AppSettings["follow_userid"].Length == 0 ? string.Empty : "&follow=" + ConfigurationManager.AppSettings["follow_userid"]) +
                                    (ConfigurationManager.AppSettings["location_coord"].Length == 0 ? string.Empty : "&locations=" + ConfigurationManager.AppSettings["location_coord"]);

            if (!string.IsNullOrEmpty(postparameters))
            {
                if (postparameters.IndexOf('&') == 0)
                    postparameters = postparameters.Remove(0, 1).Replace("#", "%23");
                postparameters = postparameters.Replace(", ", ",");
            }

            int wait = 250;

            try
            {
                StringBuilder tweetBuilder = new StringBuilder();
                int currentTweetSize = 1;
                while (true)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        Console.WriteLine("It has been cancelled");
                        break;
                    }
                    try
                    {
                        //Connect
                        if (_httpMethod == "POST")
                            webRequest = (HttpWebRequest) WebRequest.Create(new Uri(streamUrl));
                        else
                            webRequest = (HttpWebRequest) WebRequest.Create(streamUrl + GetAuthHeader(streamUrl, _customerKey, _customerSecret, _accessToken, _accessTokenSecret, _httpMethod));
                        webRequest.Timeout = -1;
                        webRequest.Method = _httpMethod;

                        if (_httpMethod == "POST")
                        {
                            webRequest.Headers.Add("Authorization", GetAuthHeader(streamUrl + "?" + postparameters, _customerKey,
                                _customerSecret, _accessToken, _accessTokenSecret, _httpMethod));
                        }

                        Encoding encode = Encoding.GetEncoding("utf-8");
                        if (postparameters.Length > 0)
                        {
                            webRequest.ContentType = "application/x-www-form-urlencoded";

                            byte[] twitterTrack = encode.GetBytes(postparameters);

                            webRequest.ContentLength = twitterTrack.Length;
                            using (Stream twitterPost = webRequest.GetRequestStream())
                            {
                                twitterPost.Write(twitterTrack, 0, twitterTrack.Length);
                                twitterPost.Close();
                            }
                        }

                        webResponse = (HttpWebResponse) webRequest.GetResponse();
                        responseStream = new StreamReader(webResponse.GetResponseStream(), encode);

                        //Read the stream.
                        while (true)
                        {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                if (!string.IsNullOrEmpty(tweetBuilder.ToString()) && !string.IsNullOrWhiteSpace(tweetBuilder.ToString()))
                                    _storage.Add(tweetBuilder.ToString());
                                break;
                            }
                            string jsonText = responseStream.ReadLine();
                            //Post each message to the queue.
                            if (!string.IsNullOrEmpty(jsonText) && !string.IsNullOrWhiteSpace(jsonText))
                            {
                                tweetBuilder.AppendLine(jsonText);
                                if (currentTweetSize >= _maxTwitterCount)
                                {
                                    var fileName = _storage.Add(tweetBuilder.ToString()); // do something with the file name?
                                    currentTweetSize = 0;
                                    tweetBuilder = new StringBuilder();
                                }
                                else
                                    currentTweetSize++;
                            }
                            //Success
                            wait = 250;
                        }
                        //Abort is needed or responseStream.Close() will hang.
                    }
                    catch (WebException ex)
                    {
                        Console.WriteLine(ex.Message);
                        if (ex.Status == WebExceptionStatus.ProtocolError)
                        {
                            //-- From Twitter Docs -- 
                            //When a HTTP error (> 200) is returned, back off exponentially. 
                            //Perhaps start with a 10 second wait, double on each subsequent failure, 
                            //and finally cap the wait at 240 seconds. 
                            //Exponential Backoff
                            if (wait < 10000)
                                wait = 10000;
                            else
                            {
                                if (wait < 240000)
                                    wait = wait*2;
                            }
                        }
                        else
                        {
                            //-- From Twitter Docs -- 
                            //When a network error (TCP/IP level) is encountered, back off linearly. 
                            //Perhaps start at 250 milliseconds and cap at 16 seconds.
                            //Linear Backoff
                            if (wait < 16000)
                                wait += 250;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    finally
                    {
                        if (webRequest != null)
                            webRequest.Abort();
                        if (responseStream != null)
                        {
                            responseStream.Close();
                            responseStream = null;
                        }

                        if (webResponse != null)
                        {
                            webResponse.Close();
                            webResponse = null;
                        }
                        Console.WriteLine("Waiting: " + wait);
                        Thread.Sleep(wait);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Waiting: " + wait);
                Thread.Sleep(wait);
            }
            Console.WriteLine("The Process is closing");
            Environment.Exit(0);
        }
    }
}