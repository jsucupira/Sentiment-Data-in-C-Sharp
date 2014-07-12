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
        public string[] Get(object filteringObject)
        {
            string filter = null;
            if (filteringObject != null)
                filter = filteringObject.ToString();
            List<string> result = new List<string>();

            if (!string.IsNullOrEmpty(filter))
            {
                long tick;
                DateTime fileDate;
                if (DateTime.TryParse(filter, out fileDate))
                {
                    DirectoryInfo directory = new DirectoryInfo(_folderPath);
                    result.AddRange(directory.GetFiles("*.json", SearchOption.TopDirectoryOnly).Where(t => t.CreationTimeUtc >= fileDate)
                        .SelectMany(t => File.ReadAllLines(t.FullName)));
                }
                else if (File.Exists(string.Format(@"{0}\{1}", _folderPath, filter)))
                {
                    result.AddRange(File.ReadAllLines(string.Format(@"{0}\{1}", _folderPath, filter)));
                }
                else if (long.TryParse(filter, out tick))
                {
                    fileDate = tick.ConvertTicksToDateTime();
                    DirectoryInfo directory = new DirectoryInfo(_folderPath);
                    result.AddRange(directory.GetFiles("*.json", SearchOption.TopDirectoryOnly).Where(t => t.CreationTimeUtc >= fileDate)
                        .SelectMany(t => File.ReadAllLines(t.FullName)));
                }
            }
            else
            {
                DirectoryInfo directory = new DirectoryInfo(_folderPath);
                result.AddRange(directory.GetFiles("*.json", SearchOption.TopDirectoryOnly).SelectMany(t => File.ReadAllLines(t.FullName)));
            }
            return result.ToArray();
        }
    }
}