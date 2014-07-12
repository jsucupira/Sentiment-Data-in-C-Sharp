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

        public object Add(string data)
        {
            long uniqueName = DateTime.Now.Ticks;
            byte[] content = Encoding.UTF8.GetBytes(data);
            return _azureBlobUtil.UploadBlob(content, _containerName, string.Format("{0}.json", uniqueName));
        }

        public string[] Get(object filteringObject)
        {
            string filter = null;
            if (filteringObject != null)
                filter = filteringObject.ToString();
            List<string> blobList = new List<string>();

            if (!string.IsNullOrEmpty(filter))
            {
                DateTime createdDt;
                long tick;
                if (DateTime.TryParse(filter, out createdDt))
                {
                    blobList.AddRange(_azureBlobUtil.BlobList(_containerName, createdDt));
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
            else
            {
                blobList.AddRange(_azureBlobUtil.BlobList(_containerName));
            }

            List<string> tweets = new List<string>();
            foreach (var blob in blobList)
            {
                var stringValue = Encoding.UTF8.GetBytes(_azureBlobUtil.DownloadBlobAsText(_containerName, blob));
                tweets.AddRange(GetArrayFromString(stringValue));
            }
            return tweets.ToArray();
        }

        private IEnumerable<string> GetArrayFromString(byte[] stringValue)
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