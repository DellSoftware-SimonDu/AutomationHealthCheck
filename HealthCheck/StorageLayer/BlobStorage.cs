using System;
using System.IO;
using System.Threading;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.IO.Compression;
using System.Threading.Tasks;

namespace StorageLayer
{
    public class BlobStorage
    {
        #region CONSTANTS
        public const string SQLSERVER_STATIC_HEALTH_CHECK_CONTAINER = "sqlserver-static-health-check";
        #endregion

        #region STATIC VARIABLES

        private static CloudBlobClient blobStorage;

        private static volatile bool initialized;
        private static readonly object initLock = new Object();
        #endregion

        public static void WriteString(string containerName, string blobName, string value)
        {
            WriteString(containerName, blobName, value, null /*blobLeaseID*/);
        }

        public static void WriteString(string containerName, string blobName, string value, string blobLeaseID)
        {
            var container = GetOrCreateContainer(containerName);
            WriteStringToExistingContainer(container, blobName, value, blobLeaseID);
        }

        public static void WriteStringToExistingContainer(CloudBlobContainer container, string blobName, string value, string blobLeaseID = null)
        {
            var subStr = string.IsNullOrWhiteSpace(value) ? "<empty>" : value.Substring(0, Math.Min(value.Length, 256)) + "...";

            AccessCondition accessCondition = null;
            if (!string.IsNullOrWhiteSpace(blobLeaseID))
            {
                accessCondition = new AccessCondition
                {
                    LeaseId = blobLeaseID,
                };
            }

            CloudBlockBlob blob = container.GetBlockBlobReference(blobName);
            blob.UploadText(value, Encoding.UTF8, accessCondition);
        }

        public virtual string ReadString(string containerName, string blobName)
        {
            var container = GetOrCreateContainer(containerName);
            var blob = container.GetBlockBlobReference(blobName);
            if (blob.Exists())
            {
                return ReadStringFromBlob(blob);
            }
            else
            {
                throw new Exception("Blob not found: " + container.Name + "/" + blobName);
            }
        }

        private static string ReadStringFromBlob(CloudBlockBlob blob)
        {
            using (var stream = new MemoryStream())
            {
                blob.DownloadToStream(stream);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        public virtual string TryReadString(string containerName, string blobName)
        {
            var container = GetOrCreateContainer(containerName);
            return TryReadString(container, blobName);
        }

        public static string TryReadString(CloudBlobContainer container, string blobName)
        {
            var blob = container.GetBlockBlobReference(blobName);
            return blob.Exists() ? ReadStringFromBlob(blob) : null;
        }

        public static void DeleteIfExists(CloudBlobContainer container, string blobName)
        {
            var blob = container.GetBlockBlobReference(blobName);
            blob.DeleteIfExists(DeleteSnapshotsOption.IncludeSnapshots);
        }

        public static void StreamSmallBlobToBlobStorage(string containerName, string blobName, Stream value)
        {
            CloudBlobContainer container = GetOrCreateContainer(containerName);
            CloudBlockBlob blob = container.GetBlockBlobReference(blobName);
            blob.UploadFromStream(value);
        }

        public static bool BlobExists(string containerName, string blobName)
        {
            CloudBlobContainer container = GetOrCreateContainer(containerName);
            CloudBlockBlob blob = container.GetBlockBlobReference(blobName);
            return blob.Exists();
        }

        public static void StreamLargeBlobToBlobStorage(string containerName, string blobName, Stream dataStream)
        {
            var cloudBlobContainer = GetOrCreateContainer(containerName);
            StreamLargeBlobToBlobStorageContainer(cloudBlobContainer, blobName, dataStream);
        }

        public static void UploadSmallBlobToBlobStorage(CloudBlobContainer cloudBlobContainer,
            string blobName, byte[] content)
        {
            using (var stream = new MemoryStream(content))
            {
                CloudBlockBlob blob = cloudBlobContainer.GetBlockBlobReference(blobName);
                for (int attempt = 1; ;)
                {
                    try
                    {
                        blob.UploadFromStream(stream, null, new BlobRequestOptions()
                        {
                            RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(2), 3)
                        });
                        break;
                    }
                    catch (Exception ex)
                    {
                        if (ex is IOException || ex is SocketException)
                        {
                            attempt++;
                            if (attempt > 3)
                            {
                                ex.Data["Attempts"] = attempt;
                                throw;
                            }
                            Thread.Sleep(2000);
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }
        }

        public static object GetOrCreateContainer(object backup, string tRACE_DATA_CONTAINER)
        {
            throw new NotImplementedException();
        }

        public static void StreamLargeBlobToBlobStorageContainer(CloudBlobContainer cloudBlobContainer, string blobName, Stream dataStream)
        {
            CloudBlockBlob blob = cloudBlobContainer.GetBlockBlobReference(blobName);
            dataStream.Position = 0;
            BinaryReader br = new BinaryReader(dataStream);

            int blockCount = 0;
            while (br.BaseStream.Position < br.BaseStream.Length)
            {
                byte[] buffer = br.ReadBytes(1024 * 1024 * 4);  //<- Read 4Mb
                var blockId = Convert.ToBase64String(BitConverter.GetBytes(blockCount));
                var memoryStream = new MemoryStream(buffer);

                blob.PutBlock(blockId, memoryStream, null);
                blockCount++;
            }

            if (blockCount > 0)
            {
                string[] blockIDS = new string[blockCount];
                for (int i = 0; i < blockCount; i++)
                {
                    blockIDS[i] = Convert.ToBase64String(BitConverter.GetBytes(i));
                }

                // Ensure that all of the blobkIDs are strings of the same length.
                // Otherwise Azure will barf with "the specified block list is invalid"
                var blockIDLength = blockIDS.First().Length;
                if (blockIDS.Any(id => id.Length != blockIDLength))
                {
                    var badBlockID = blockIDS.First(id => id.Length != blockIDLength);
                    throw new ApplicationException(string.Format("When streaming a large blob to the Azure blob-store, the list of {0} blob block-ids " +
                                                                 "should contain 'id strings' whose length are all equal (i.e. {1}).\n" +
                                                                 "However, an id string was detected with a different length. The bad id is \"{2}\".\n" +
                                                                 "The blob is {3]/{4}"
                                                                , blockIDS.Length
                                                                , blockIDLength
                                                                , badBlockID
                                                                , cloudBlobContainer.Name
                                                                , blobName
                                                                ));
                }

                blob.PutBlockList(blockIDS);
            }
        }

        public static CloudBlockBlob GetBlockBlobReference(string containerName, string blobName)
        {
            var storageAccount = StorageAccounts.Instance.Default;

            CloudBlobClient client = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer container = client.GetContainerReference(containerName);
            CloudBlockBlob blob = container.GetBlockBlobReference(blobName);
            if (blob.Exists())
            {
                return blob;
            }
            else
            {
                throw new Exception("Blob not found: " + containerName + "/" + blobName);
            }
        }

        public static long GetBlobSize(IListBlobItem blob)
        {
            var blobName = GetBlobNameFromUri(blob);
            var blobRef = blob.Container.GetBlockBlobReference(blobName);
            if (blobRef == null)
                throw new ApplicationException("Unable to locate the blob: " + blob.Uri.AbsoluteUri);

            blobRef.FetchAttributes();
            return blobRef.Properties.Length;
        }

        public static MemoryStream GetBlobToStream(string containerName, string blobName)
        {
            CloudBlobContainer container = GetOrCreateContainer(containerName);
            return GetBlobToStream(container, blobName);
        }

        public static MemoryStream GetBlobToStream(CloudBlobContainer container, string blobName)
        {
            CloudBlockBlob blob = container.GetBlockBlobReference(blobName);
            if (!blob.Exists())
                return null;

            MemoryStream ms = new MemoryStream();
            blob.DownloadToStream(ms, null, new BlobRequestOptions()
            {
                RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(2), 3)
            });

            ms.Position = 0;

            return ms;
        }

        public static string TryLoadFingerprintFromBlob(string containerName, string blobName, Encoding encoding)
        {
            using (var memoryStream = GetBlobToStream(containerName, blobName))
            {
                if (memoryStream == null)
                    return null;

                using (StreamReader reader = new StreamReader(memoryStream, encoding))
                {
                    string text = reader.ReadToEnd();
                    return text;
                }
            }
        }

        public static void Initialize()
        {
            if (initialized)
                return;

            lock (initLock)
            {
                if (initialized)
                    return;

                while (!initialized)
                {
                    try
                    {
                        var storageAccount = StorageAccounts.Instance.Default;

                        blobStorage = storageAccount.CreateCloudBlobClient();
                        initialized = true;
                    }
                    catch (StorageException ex)
                    {
                        if (ex.Message != "Unable to connect to the remote server")
                            throw;
                        Thread.Sleep(5000);
                    }
                }
            }
        }

        public static void Uninitialize()
        {
            //log.Verbose(Logger.TraceEventID.traceFunctionEntry, "Uninitialize()");

            if (!initialized)
                return;
            lock (initLock)
            {
                if (!initialized)
                    return;

                initialized = false;
                blobStorage = null;
            }
        }

        public static CloudBlobContainer GetContainer(string containerName)
        {
            //log.Verbose(Logger.TraceEventID.traceFunctionEntry, "GetContainer({0})", containerName);
            Initialize();
            return blobStorage.GetContainerReference(containerName);
        }

        public virtual CloudBlobContainer GetContainer(CloudStorageAccount storageAccount, string containerName)
        {
            //log.Verbose(Logger.TraceEventID.traceFunctionEntry, "GetContainer({0}, {1})", storageAccount, containerName);
            Initialize();
            var blobStorage = storageAccount.CreateCloudBlobClient();
            return blobStorage.GetContainerReference(containerName);
        }

        public static CloudBlobContainer GetOrCreateContainer(CloudStorageAccount storageAccount, string containerName)
        {
            var blobStorage = storageAccount.CreateCloudBlobClient();
            return _GetOrCreateContainer(blobStorage, containerName);
        }

        public static CloudBlobContainer GetOrCreateContainer(string containerName)
        {
            //log.Verbose(Logger.TraceEventID.traceFunctionEntry, "GetOrCreateContainer({0})", containerName);

            Initialize();   //<- Initializes the static property 'blobStorage'

            return _GetOrCreateContainer(blobStorage, containerName);   //<- Pass the static property 'blobStorage' - which refers to the 'default' blob-store
        }

        private static CloudBlobContainer _GetOrCreateContainer(CloudBlobClient cloudBlobClient, string containerName)
        {
            // find or create requested container
            CloudBlobContainer container = cloudBlobClient.GetContainerReference(containerName);

            if (container.CreateIfNotExists(new BlobRequestOptions()
            {
                RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(5), 12)
            }))
            {
                container.SetPermissions(
                    new BlobContainerPermissions
                    {
                        PublicAccess = BlobContainerPublicAccessType.Off
                    });
            }

            return container;
        }

        public static bool ContainerExists(string containerName)
        {
            Initialize();

            CloudBlobContainer container = blobStorage.GetContainerReference(containerName);
            return container.Exists();
        }

        public static string GetBlobStoreName()
        {
            Initialize();
            var result = blobStorage.BaseUri.AbsolutePath;
            if (result.StartsWith("/"))
                result = result.Substring(1);
            return result;
        }

        public static string GetBlobNameFromUri(IListBlobItem blob)
        {
            var searchFor = "/" + blob.Container.Name + "/";
            var i = blob.Uri.AbsoluteUri.LastIndexOf(searchFor);
            var result = blob.Uri.AbsoluteUri;
            return result.Substring(i + searchFor.Length, result.Length - i - searchFor.Length);
        }

        public static string GetBlobNameFromWasbUri(string uri)
        {
            // E.g. Input is: "wasb://db-ss-smf@lucyhadooptest.blob.core.windows.net/descendant/output/7749/20150326-054121-5569"
            //        Yields: "descendant/output/7749/20150326-054121-5569"
            var searchFor = ".blob.core.windows.net/";
            var p = uri.ToLowerInvariant().LastIndexOf(searchFor);
            if (p > 0)
                return uri.Substring(p + searchFor.Length);

            return string.Empty;
        }
    }
}
