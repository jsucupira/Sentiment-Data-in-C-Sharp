using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Core.Common.Helpers;
using Data.Contracts;
using Domain;
using Newtonsoft.Json;

namespace TwitterService
{
    public class ParseTwitterData
    {
        private static readonly string _affinFilePath = ConfigurationManager.AppSettings["afinn_file"];

        #region lookups

        private static readonly Dictionary<string, string> _tweetLookupDictionary = new Dictionary<string, string>
        {
            {"coordinates", "Coordinates"},
            {"created_at", "CreatedDate"},
            {"favorite_count", "FavoritedCount"},
            {"favorited", "Favorited"},
            {"id_str", "Identifier"},
            {"lang", "Language"},
            {"place", "Place"},
            {"source", "Source"},
            {"text", "Text"}
        };

        private static readonly Dictionary<string, string> _userLookupDictionary = new Dictionary<string, string>
        {
            {"created_at", "CreatedDate"},
            {"description", "Description"},
            {"friends_count", "FriendsCount"},
            {"geo_enabled", "GeoEnabled"},
            {"id_str", "Identifier"},
            {"lang", "Language"},
            {"location", "Location"},
            {"name", "Name"},
            {"profile_image_url", "ProfileImageUrl"},
            {"time_zone", "TimeZone"},
            {"url", "Url"},
            {"screen_name", "UserName"},
            {"utc_offset", "UtcOffset"}
        };

        private static readonly Dictionary<string, float> _scoreDictionary = new Dictionary<string, float>();

        #endregion

        static ParseTwitterData()
        {
            if (File.Exists(_affinFilePath))
            {
                IEnumerable<string> fileContent = File.ReadLines(_affinFilePath);
                foreach (var line in fileContent)
                {
                    var scores = Regex.Split(line, "\t");
                    if (scores.Length > 1)
                    {
                        float value;
                        if (float.TryParse(scores[1], out value) && !string.IsNullOrEmpty(scores[0]))
                            _scoreDictionary.Add(scores[0].ToLower(), value);
                    }
                }
            }
        }

        private static SocialData ParceTweetsIntoObject(string jsonData)
        {
            SocialData socialData = new SocialData();
            Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonData);

            foreach (var item in dictionary)
            {
                if (_tweetLookupDictionary.ContainsKey(item.Key))
                {
                    object propValu = item.Value;
                    if (propValu != null)
                        ReflectionUtils.SetProperty(socialData, _tweetLookupDictionary[item.Key], propValu.ToString());
                }
            }
            User user = new User();
            if (dictionary.ContainsKey("user"))
            {
                object userData = dictionary["user"];
                if (userData != null)
                {
                    Dictionary<string, object> userDic = JsonConvert.DeserializeObject<Dictionary<string, object>>(userData.ToString());
                    foreach (var item in userDic)
                    {
                        if (_userLookupDictionary.ContainsKey(item.Key))
                        {
                            object propValu = item.Value;
                            if (propValu != null)
                                ReflectionUtils.SetProperty(user, _userLookupDictionary[item.Key], propValu.ToString());
                        }
                    }
                }
                else
                    return null;
            }

            socialData.User = user;
            return socialData;
        }

        private static string ParseTweetText(string jsonData)
        {
            Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonData);
            string tweet = string.Empty;
            if (dictionary.ContainsKey("text"))
            {
                object tweetObject = dictionary["text"];
                if (tweetObject != null && !string.IsNullOrEmpty(tweetObject.ToString()))
                    tweet = tweetObject.ToString();
            }
            return tweet;
        }
        private static Dictionary<string, object> GetTweetDictionary(string jsonData)
        {
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonData);
        }

        public static List<SocialData> RetrieveTweetObjects(IStorage storage, string filter = null)
        {
            IEnumerable<string> getAllTweets = storage.Get(filter);

            List<SocialData> socialDataList = new List<SocialData>();
            foreach (var tweet in getAllTweets)
            {
                if (!string.IsNullOrEmpty(tweet))
                {
                    var item = ParceTweetsIntoObject(tweet);
                    if (item != null && !string.IsNullOrEmpty(item.Text))
                        socialDataList.Add(item);
                }
            }
            return socialDataList;
        }

        public static float RetrieveTweetsScores(IStorage storage, object filter = null)
        {
            IEnumerable<string> getAllTweets = storage.Get(filter);
            List<string> tweetList = new List<string>();

            foreach (var tweet in getAllTweets)
            {
                if (!string.IsNullOrEmpty(tweet))
                {
                    var item = ParseTweetText(tweet);
                    if (item != null && !string.IsNullOrEmpty(item))
                        tweetList.Add(item);
                }
            }

            float totalScore = 0;
            foreach (var socialData in tweetList)
            {
                totalScore += ScoreEachTweet(socialData).Item2;
            }
            return totalScore;
        }

        private static Tuple<string, float> ScoreEachTweet(string tweet)
        {
            var tweetWords = tweet.Split(' ');
            float totalScore = 0;
            foreach (var word in tweetWords)
            {
                if (!string.IsNullOrWhiteSpace(word))
                {
                    string wordToLower = word.ToLower();
                    if (_scoreDictionary.ContainsKey(wordToLower))
                        totalScore += _scoreDictionary[wordToLower];
                }
            }
            return Tuple.Create(tweet, totalScore);
        }

        public static Dictionary<string, float> RetrieveTermSentiments(IStorage storage, object filter = null)
        {
            IEnumerable<string> getAllTweets = storage.Get(filter);
            List<string> tweetList = new List<string>();

            foreach (var tweet in getAllTweets)
            {
                if (!string.IsNullOrEmpty(tweet) && !string.IsNullOrWhiteSpace(tweet))
                {
                    var item = ParseTweetText(tweet);
                    if (item != null && !string.IsNullOrEmpty(item))
                        tweetList.Add(item);
                }
            }

            Dictionary<string, float> nonSentiments = new Dictionary<string, float>(tweetList.Count);
            Dictionary<string, float> termsAndScores = new Dictionary<string, float>(tweetList.Count);
            Dictionary<string, float> totalAppearance = new Dictionary<string, float>(tweetList.Count);

            foreach (var socialData in tweetList)
            {
                float tweetScore = 0;
                tweetScore += ScoreEachTweet(socialData).Item2;

                var tweetWords = socialData.Split(' ');
                foreach (var word in tweetWords)
                {
                    if (!string.IsNullOrWhiteSpace(word) && !string.IsNullOrWhiteSpace(word))
                    {
                        string wordToLower = word.ToLower();
                        if (!_scoreDictionary.ContainsKey(wordToLower))
                        {
                            if (nonSentiments.ContainsKey(wordToLower))
                            {
                                nonSentiments[wordToLower] += tweetScore;
                                totalAppearance[wordToLower]++;
                            }
                            else
                            {
                                nonSentiments.Add(wordToLower, tweetScore);
                                totalAppearance.Add(wordToLower, 1);
                            }
                        }
                    }
                }
            }

            foreach (var nonSentiment in nonSentiments)
            {
                termsAndScores.Add(nonSentiment.Key, (nonSentiment.Value / totalAppearance[nonSentiment.Key]));
            }
            return termsAndScores;
        }

        public static Dictionary<string, float> RetrieveScoresByUSStates(IStorage storage, object filter = null)
        {
            IEnumerable<string> getAllTweets = storage.Get(filter);
            List<Dictionary<string, object>> tweetDicList = new List<Dictionary<string, object>>();
            Dictionary<string, List<string>> tweetsByState = new Dictionary<string, List<string>>();
            var lookupStateList = Utility.StateList();
            Dictionary<string, float> stateScores = new Dictionary<string, float>();

            foreach (var tweet in getAllTweets)
                tweetDicList.Add(GetTweetDictionary(tweet));

            foreach (var tweet in tweetDicList)
            {
                if (tweet.ContainsKey("user"))
                {
                    var userObject = tweet["user"];
                    if (userObject != null)
                    {
                        var user = JsonConvert.DeserializeObject<Dictionary<string, object>>(userObject.ToString());
                        if (user != null && user.ContainsKey("location"))
                        {
                            var locationObject = user["location"] as string;


                            if (!string.IsNullOrEmpty(locationObject))
                            {
                                string[] location = locationObject.Split(',');
                                var currentState = GetStateValue(location, lookupStateList);
                                if (!string.IsNullOrEmpty(currentState))
                                {
                                    if (!tweetsByState.ContainsKey(currentState))
                                        tweetsByState.Add(currentState, new List<string>());

                                    tweetsByState[currentState].Add(tweet["text"] as string);
                                }
                            }
                        }
                    }
                }
            }

            foreach (var state in tweetsByState.Where(t => t.Value.Any()))
            {
                float tweetScore = 0;
                foreach (var tweetList in state.Value)
                {
                    tweetScore += ScoreEachTweet(tweetList).Item2;
                }
                stateScores[state.Key] = tweetScore / state.Value.Count;

            }

            return stateScores;
        }

        private static string GetStateValue(string[] location, Dictionary<string, string> stateDictionary)
        {
            if (location.Length > 1)
            {
                location[0] = location[0].Trim();
                location[1] = location[1].Trim();
                if (stateDictionary.ContainsKey(location[1].ToUpper()))
                    return location[1].ToUpper();
                else if (stateDictionary.ContainsKey(location[0].ToUpper()))
                    return location[0].ToUpper();
                else if (stateDictionary.ContainsValue(location[1].ToLower()))
                    return stateDictionary.FirstOrDefault(t => t.Value == location[1].ToLower()).Key;
                else if (stateDictionary.ContainsValue(location[0].ToLower()))
                    return stateDictionary.FirstOrDefault(t => t.Value == location[0].ToLower()).Key;
            }
            else
            {
                location[0] = location[0].Trim();
                if (stateDictionary.ContainsKey(location[0].ToUpper()))
                    return location[0].ToUpper();
                else if (stateDictionary.ContainsValue(location[0].ToLower()))
                    return stateDictionary.FirstOrDefault(t => t.Value == location[0].ToLower()).Key;
            }

            return string.Empty; //no state found
        }
    }
}