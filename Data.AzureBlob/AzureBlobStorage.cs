using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using Core.Common.Helpers;
using Data.Contracts;

namespace Data.AzureBlob
{
    [Export(typeof(IStorage))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class AzureBlobStorage : IStorage
    {
        private static readonly string _connectionString = ConfigurationManager.AppSettings["azure_blob_connection"];
        private static readonly string _containerName = ConfigurationManager.AppSettings["azure_container"];
        private readonly AzureBlobUtil _azureBlobUtil = new AzureBlobUtil(_connectionString);
        private readonly string _totalTweetCount = ConfigurationManager.AppSettings["max_tweet_to_analyse"];

        public object Add(string data)
        {
            long uniqueName = DateTime.Now.Ticks;
            byte[] content = Encoding.UTF8.GetBytes(data);
            return _azureBlobUtil.UploadBlob(content, _containerName, string.Format("{0}.json", uniqueName));
        }

        public IEnumerable<string> Get(object filteringObject)
        {
            string[] filterArray = null;
            string filter = null;
            if (filteringObject != null)
            {
                if (filteringObject.GetType() == typeof(string[]))
                {
                    filterArray = (string[])filteringObject;
                }

                if (filterArray != null)
                    filter = filteringObject.ToString();
            }

            int maxCount;
            int currentCount = 0;
            var capExists = int.TryParse(_totalTweetCount, out maxCount);
            List<string> blobList = new List<string>();

            if (!string.IsNullOrEmpty(filter))
            {
                DateTime startDt;
                long tick;
                if (DateTime.TryParse(filter, out startDt))
                {
                    blobList.AddRange(_azureBlobUtil.BlobList(_containerName, startDt));
                }
                else if (long.TryParse(filter, out tick))
                {
                    var fileDate = tick.ConvertTicksToDateTime();
                    blobList.AddRange(_azureBlobUtil.BlobList(_containerName, fileDate));
                }
                else
                {
                    var stringValue = Encoding.UTF8.GetBytes(_azureBlobUtil.DownloadBlobAsText(_containerName, filter));
                    blobList.AddRange(GetArrayFromString(stringValue));
                    return blobList.ToArray();
                }
            }
            else if (filterArray != null)
            {
                DateTime startDt;
                DateTime endDt;
                long startTick;
                long endTick;
                if (DateTime.TryParse(filterArray[0], out startDt) && DateTime.TryParse(filterArray[1], out endDt))
                {
                    blobList.AddRange(_azureBlobUtil.BlobList(_containerName, startDt, endDt));
                }
                else if (long.TryParse(filterArray[0], out startTick) && long.TryParse(filterArray[1], out endTick))
                {
                    startDt = startTick.ConvertTicksToDateTime();
                    endDt = endTick.ConvertTicksToDateTime();
                    blobList.AddRange(_azureBlobUtil.BlobList(_containerName, startDt, endDt));
                }
            }
            else
            {
                blobList.AddRange(_azureBlobUtil.BlobList(_containerName));
            }

            List<string> tweets = new List<string>();
            foreach (var blob in blobList)
            {
                if (capExists && currentCount >= maxCount)
                    break;

                var stringValue = Encoding.UTF8.GetBytes(_azureBlobUtil.DownloadBlobAsText(_containerName, blob));
                tweets.AddRange(GetArrayFromString(stringValue));
                currentCount = tweets.Count;
            }
            return tweets.ToArray();
        }

        private static IEnumerable<string> GetArrayFromString(byte[] stringValue)
        {
            List<string> blobList = new List<string>();
            using (MemoryStream stream = new MemoryStream((stringValue)))
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8, true, stringValue.Length))
            {
                var currentLine = reader.ReadLine();
                while (!string.IsNullOrEmpty(currentLine))
                {
                    blobList.Add(currentLine);
                    currentLine = reader.ReadLine();
                }
            }
            return blobList;
        }
    }
}