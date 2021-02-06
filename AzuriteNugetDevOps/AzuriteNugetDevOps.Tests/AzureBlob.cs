using Azure.Storage.Blobs;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace AzuriteNugetDevOps.Tests
{
    [CollectionDefinition("AzureBlob")]
    public class BaseCollection : ICollectionFixture<AzureBlobStorageFixture>
    {
    }

    [Collection("AzureBlob")]
    public abstract class AzureBlob
    {
        Random rand = new Random();

        protected BlobServiceClient serviceClient;
        protected AzureBlobStorageProvider provider;

        protected Func<string, Task<bool>> ContainerExistsAsync => async (c)
            => await serviceClient.GetBlobContainerClient(c).ExistsAsync();

        protected Func<string, string, Task<bool>> BlobExistsAsync => async (c, b)
            => await serviceClient.GetBlobContainerClient(c).GetBlobClient(b).ExistsAsync();

        public AzureBlob(AzureBlobStorageFixture fixture)
        {
            serviceClient = fixture.ServiceClient;
            provider = new AzureBlobStorageProvider(fixture.ConnectionString);
        }

        protected byte[] GenerateRandomBlob(int length = 256)
        {
            var buffer = new byte[length];
            rand.NextBytes(buffer);
            return buffer;
        }

        protected MemoryStream GenerateRandomBlobStream(int length = 256)
        {
            return new MemoryStream(GenerateRandomBlob(length));
        }

        protected string GetRandomContainerName()
        {
            return AzureBlobStorageFixture.ContainerPrefix + Guid.NewGuid().ToString("N");
        }

        protected string GetRandomBlobName()
        {
            return Guid.NewGuid().ToString("N");
        }

        protected bool StreamEquals(Stream stream1, Stream stream2)
        {
            if (stream1.CanSeek)
            {
                stream1.Seek(0, SeekOrigin.Begin);
            }
            if (stream2.CanSeek)
            {
                stream2.Seek(0, SeekOrigin.Begin);
            }

            const int bufferSize = 2048;

            byte[] buffer1 = new byte[bufferSize];
            byte[] buffer2 = new byte[bufferSize];

            while (true)
            {
                int count1 = stream1.Read(buffer1, 0, bufferSize);
                int count2 = stream2.Read(buffer2, 0, bufferSize);

                if (count1 != count2)
                    return false;

                if (count1 == 0)
                    return true;

                if (!buffer1.Take(count1).SequenceEqual(buffer2.Take(count2)))
                    return false;
            }
        }

        protected void NoExceptionThrown<T>(Action a) where T : Exception
        {
            try
            {
                a();
            }
            catch (T)
            {
                Assert.NotEqual("Expected no {0} to be thrown", typeof(T).Name);
            }
        }
    }
}
