using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using Core.Common.Helpers;
using Data.Contracts;

namespace Data.FileSystem
{
    [Export(typeof(IStorage))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class FileSystemStorage : IStorage
    {
        private readonly string _folderPath = ConfigurationManager.AppSettings["folder_path"];
        private readonly string _totalTweetCount = ConfigurationManager.AppSettings["max_tweet_to_analyse"];

        public FileSystemStorage()
        {
            if (!Directory.Exists(_folderPath))
                Directory.CreateDirectory(_folderPath);
        }

        public object Add(string data)
        {
            long uniqueName = DateTime.Now.Ticks;
            string fileFullName = string.Format(@"{0}\{1}.json", _folderPath, uniqueName);
            File.AppendAllText(fileFullName, data);
            return fileFullName;
        }

        /// <summary>
        ///     pass in datetime or Full File Name
        /// Time is UTC
        /// </summary>
        /// <param name="filteringObject">pass in datetime or Full File Name</param>
        /// <returns></returns>
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

                if (filterArray == null)
                    filter = filteringObject.ToString();
            }

            List<string> result = new List<string>();
            int maxCount;
            int currentCount = 0;
            var capExists = int.TryParse(_totalTweetCount, out maxCount);

            if (!string.IsNullOrEmpty(filter))
            {
                long tick;
                DateTime fileDate;
                if (DateTime.TryParse(filter, out fileDate))
                {
                    DirectoryInfo directory = new DirectoryInfo(_folderPath);
                    var totalFiles = directory.GetFiles("*.json", SearchOption.TopDirectoryOnly).Where(t => t.CreationTime >= fileDate);

                    foreach (var totalFile in totalFiles)
                    {
                        if (capExists && currentCount >= maxCount)
                            break;

                        result.AddRange(File.ReadAllLines(totalFile.FullName));
                        currentCount = result.Count;
                    }

                }
                else if (File.Exists(string.Format(@"{0}\{1}", _folderPath, filter)))
                {
                    result.AddRange(File.ReadAllLines(string.Format(@"{0}\{1}", _folderPath, filter)));
                }
                else if (long.TryParse(filter, out tick))
                {
                    fileDate = tick.ConvertTicksToDateTime();
                    DirectoryInfo directory = new DirectoryInfo(_folderPath);
                    var totalFiles = directory.GetFiles("*.json", SearchOption.TopDirectoryOnly).Where(t => t.CreationTime >= fileDate);

                    foreach (var totalFile in totalFiles)
                    {
                        if (capExists && currentCount >= maxCount)
                            break;

                        result.AddRange(File.ReadAllLines(totalFile.FullName));
                        currentCount = result.Count;
                    }
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
                    DirectoryInfo directory = new DirectoryInfo(_folderPath);
                    var totalFiles = directory.GetFiles("*.json", SearchOption.TopDirectoryOnly).Where(t => t.CreationTime >= startDt && t.CreationTime <= endDt);
                    foreach (var totalFile in totalFiles)
                    {
                        if (capExists && currentCount >= maxCount)
                            break;

                        result.AddRange(File.ReadAllLines(totalFile.FullName));
                        currentCount = result.Count;
                    }
                }
                else if (long.TryParse(filterArray[0], out startTick) && long.TryParse(filterArray[1], out endTick))
                {
                    startDt = startTick.ConvertTicksToDateTime();
                    endDt = endTick.ConvertTicksToDateTime();
                    DirectoryInfo directory = new DirectoryInfo(_folderPath);
                    var totalFiles = directory.GetFiles("*.json", SearchOption.TopDirectoryOnly).Where(t => t.CreationTime >= startDt && t.CreationTime <= endDt);

                    foreach (var totalFile in totalFiles)
                    {
                        if (capExists && currentCount >= maxCount)
                            break;

                        result.AddRange(File.ReadAllLines(totalFile.FullName));
                        currentCount = result.Count;
                    }
                }
            }
            else
            {
                DirectoryInfo directory = new DirectoryInfo(_folderPath);
                var totalFiles = directory.GetFiles("*.json", SearchOption.TopDirectoryOnly);

                foreach (var totalFile in totalFiles)
                {
                    if (capExists && currentCount >= maxCount)
                        break;

                    result.AddRange(File.ReadAllLines(totalFile.FullName));
                    currentCount = result.Count;
                }
            }
            return result.ToArray();
        }
    }
}