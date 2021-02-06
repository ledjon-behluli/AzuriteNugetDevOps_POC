using System;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using static AzuriteNugetDevOps.Tests.AzureStorageEmulator;

namespace AzuriteNugetDevOps.Tests
{
    public class AzureBlobStorageFixture
    {
        public const string ContainerPrefix = "storagetest-";
        public string ConnectionString => Environment.GetEnvironmentVariable("AZURE_STORAGE_CONN_STRING");

        public BlobServiceClient ServiceClient;

        public AzureBlobStorageFixture()
        {
            InitializeEmulator();
            ServiceClient = new BlobServiceClient(ConnectionString);
        }

        public async Task Dispose()
        {
            var containers = ServiceClient
                .GetBlobContainersAsync(prefix: ContainerPrefix)
                .AsPages(default, 100);

            await foreach (Azure.Page<BlobContainerItem> containerPage in containers)
            {
                foreach (BlobContainerItem containerItem in containerPage.Values)
                {
                    await ServiceClient.DeleteBlobContainerAsync(containerItem.Name);
                }
            }

            DockerProcess.StopEmulator();
        }

        private void InitializeEmulator()
        {
            var isBuild = Environment.GetEnvironmentVariable("TF_BUILD");
            if (!string.IsNullOrEmpty(isBuild))
            {
                var hostIp = Environment.GetEnvironmentVariable("AZS_CONTAINER_IP");
                var connectionString = string.Format("DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://{0}:10000/devstoreaccount1;", hostIp);
                Environment.SetEnvironmentVariable("AZURE_STORAGE_CONN_STRING", connectionString);
            }
            else
            {
                Environment.SetEnvironmentVariable("AZURE_STORAGE_CONN_STRING", "UseDevelopmentStorage=true");
                DockerProcess.StartEmulator();
            }
        }
    }
}
