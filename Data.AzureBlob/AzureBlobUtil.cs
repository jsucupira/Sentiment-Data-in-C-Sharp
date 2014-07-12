using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Data.AzureBlob
{
    public class AzureBlobUtil
    {
        public AzureBlobUtil(string accountConnetingString)
        {
            _storageAccount = CloudStorageAccount.Parse(accountConnetingString);
        }

        private const int MAX_BLOCK_SIZE = 4000000; // Approx. 4MB chunk size
        private readonly CloudStorageAccount _storageAccount;

        public IEnumerable<string> BlobList(string containerName)
        {
            List<string> list = new List<string>();
            // Retrieve storage account from connection string.
            // Create the blob client. 
            CloudBlobClient blobClient = _storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);

            // Loop over items within the container and output the length and URI.
            foreach (IListBlobItem item in container.ListBlobs(null, false))
            {
                CloudBlockBlob blockBlob = item as CloudBlockBlob;
                if (blockBlob != null)
                {
                    CloudBlockBlob blob = blockBlob;
                    list.Add(blob.Name);
                }
            }
            return list;
        }

        public IEnumerable<string> BlobList(string containerName, DateTime startDateTime, DateTime? enDateTime = null)
        {
            List<string> list = new List<string>();
            // Retrieve storage account from connection string.
            // Create the blob client. 
            CloudBlobClient blobClient = _storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);


            IEnumerable<CloudBlockBlob> blobList = null;
            if (enDateTime == null)
                blobList = container.ListBlobs().OfType<CloudBlockBlob>().Where(t => t.Properties.LastModified >= startDateTime && t.Name.EndsWith(".json"));
            else
                blobList = container.ListBlobs().OfType<CloudBlockBlob>().Where(t => t.Properties.LastModified >= startDateTime && t.Properties.LastModified <= enDateTime.Value && t.Name.EndsWith(".json"));

            // Loop over items within the container and output the length and URI.
            foreach (CloudBlockBlob item in blobList)
            {
                CloudBlockBlob blockBlob = item;
                if (blockBlob != null)
                {
                    CloudBlockBlob blob = blockBlob;
                    list.Add(blob.Name);
                }
            }
            return list;
        }

        public bool CheckIfFileExists(string containerName, string fileName)
        {
            // Create the blob client.
            CloudBlobClient blobClient = _storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);

            // Retrieve reference to a blob named "myblob.txt"
            CloudBlockBlob blockBlob2 = container.GetBlockBlobReference(fileName);
            try
            {
                blockBlob2.FetchAttributes();
                return true;
            }
            catch (StorageException e)
            {
                if (e.Message.Contains("404"))
                    return false;
                throw;
            }
        }

        public void DeleteBlobs(string containerName, string blobName)
        {
            // Retrieve storage account from connection string.
            // Create the blob client.
            CloudBlobClient blobClient = _storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);

            // Retrieve reference to a blob named "myblob.txt".
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);

            // Delete the blob.
            blockBlob.Delete();
        }

        public void DownloadBlobAsFile(string containerName, string filePath, string fileName)
        {
            string content = DownloadBlobAsText(containerName, fileName);

            FileInfo file = new FileInfo(filePath);
            if (file.Directory != null) file.Directory.Create(); // If the directory already exists, this method does nothing.
            File.AppendAllText(file.FullName, content, Encoding.UTF8);
        }

        public string DownloadBlobAsText(string containerName, string fileName)
        {
            // Create the blob client.
            CloudBlobClient blobClient = _storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);

            // Retrieve reference to a blob named "myblob.txt"
            CloudBlockBlob blockBlob2 = container.GetBlockBlobReference(fileName);

            string text;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                blockBlob2.DownloadToStream(memoryStream);
                text = Encoding.UTF8.GetString(memoryStream.ToArray());
            }
            return text;
        }

        private IEnumerable<FileBlock> GetFileBlocks(byte[] fileContent)
        {
            HashSet<FileBlock> hashSet = new HashSet<FileBlock>();
            if (fileContent.Length == 0)
                return new HashSet<FileBlock>();

            int blockId = 0;
            int ix = 0;

            int currentBlockSize = MAX_BLOCK_SIZE;

            while (currentBlockSize == MAX_BLOCK_SIZE)
            {
                if ((ix + currentBlockSize) > fileContent.Length)
                    currentBlockSize = fileContent.Length - ix;

                byte[] chunk = new byte[currentBlockSize];
                Array.Copy(fileContent, ix, chunk, 0, currentBlockSize);

                hashSet.Add(
                    new FileBlock
                    {
                        Content = chunk,
                        Id = Convert.ToBase64String(BitConverter.GetBytes(blockId))
                    });

                ix += currentBlockSize;
                blockId++;
            }

            return hashSet;
        }

        public Uri UploadBlob(string filePath, string containerName)
        {
            byte[] fileContent = File.ReadAllBytes(filePath);
            string blobName = Path.GetFileName(filePath);

            return UploadBlob(fileContent, containerName, blobName);
        }

        public Uri UploadBlob(byte[] fileContent, string containerName, string blobName)
        {
            CloudBlobClient blobclient = _storageAccount.CreateCloudBlobClient();

            CloudBlobContainer container = blobclient.GetContainerReference(containerName);

            CloudBlockBlob blob = container.GetBlockBlobReference(blobName);

            HashSet<string> blocklist = new HashSet<string>();
            foreach (FileBlock block in GetFileBlocks(fileContent))
            {
                blob.PutBlock(block.Id, new MemoryStream(block.Content, true), null);
                blocklist.Add(block.Id);
            }

            blob.PutBlockList(blocklist);

            return blob.Uri;
        }
    }
}