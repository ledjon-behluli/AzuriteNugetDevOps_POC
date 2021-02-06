using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AzuriteNugetDevOps
{
    public class AzureBlobStorageProvider
    {
        private readonly BlobServiceClient serviceClient;

        public AzureBlobStorageProvider(string connectionString)
        {
            serviceClient = new BlobServiceClient(connectionString);
        }

        public async Task CreateContainerAsync(string containerName)
        {
            try
            {
                await serviceClient
                       .GetBlobContainerClient(containerName)
                       .CreateIfNotExistsAsync(PublicAccessType.Blob);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        public async Task DeleteContainerAsync(string containerName)
        {
            try
            {
                await serviceClient
                       .GetBlobContainerClient(containerName)
                       .DeleteIfExistsAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        public async Task SaveBlobStreamAsync(string containerName, string blobName, Stream source, bool closeStream = true)
        {
            try
            {
                var container = serviceClient.GetBlobContainerClient(containerName);

                if (!await container.ExistsAsync().ConfigureAwait(false))
                {
                    await container.CreateIfNotExistsAsync(PublicAccessType.Blob);
                }
                else
                {
                    var blobAccessType = (await container.GetAccessPolicyAsync()).Value.BlobPublicAccess;

                    // Elevate container permissions if necessary.
                    if (blobAccessType == PublicAccessType.None)
                    {
                        await container.SetAccessPolicyAsync(PublicAccessType.Blob);
                    }
                }

                var blob = container.GetBlobClient(blobName);
                await blob.UploadAsync(source);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
            finally
            {
                if (closeStream)
                {
                    source.Dispose();
                }
            }
        }

        public async Task DeleteBlobAsync(string containerName, string blobName)
        {
            try
            {
                await serviceClient
                        .GetBlobContainerClient(containerName)
                        .DeleteBlobIfExistsAsync(blobName, DeleteSnapshotsOption.None);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }
    }
}
