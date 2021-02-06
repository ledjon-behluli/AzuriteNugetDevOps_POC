using Xunit;
using System;

namespace AzuriteNugetDevOps.Tests
{
    public class AzureBlobStorageProviderTests : AzureBlob
    {
        public AzureBlobStorageProviderTests(AzureBlobStorageFixture fixture)
            : base(fixture)
        {

        }

        #region Create

        [Fact]
        public async void CreateContainer_Method_Should_Create_A_Container()
        {
            var containerName = GetRandomContainerName();

            Assert.False(await ContainerExistsAsync(containerName));

            await provider.CreateContainerAsync(containerName);

            Assert.True(await ContainerExistsAsync(containerName));
        }

        [Fact]
        public async void SaveBlobStream_Method_Should_Create_A_Blob_And_A_Container_If_It_Doesnt_Already_Exist()
        {
            var containerName = GetRandomContainerName();
            var blob = GetRandomBlobName();

            Assert.False(await ContainerExistsAsync(containerName));

            await provider.SaveBlobStreamAsync(containerName, blob, GenerateRandomBlobStream());

            Assert.True(await ContainerExistsAsync(containerName));
            Assert.True(await BlobExistsAsync(containerName, blob));
        }

        [Fact]
        public async void Created_Blob_Content_Should_Be_The_Same_As_The_Source_Content()
        {
            var containerName = GetRandomContainerName();
            var blobName = GetRandomBlobName();
            var data = GenerateRandomBlobStream();

            await provider.SaveBlobStreamAsync(containerName, blobName, data, closeStream: false);

            var blob = serviceClient.GetBlobContainerClient(containerName).GetBlobClient(blobName);
            var stream = (await blob.DownloadAsync()).Value.Content;

            Assert.True(StreamEquals(data, stream));
        }

        #endregion

        #region Delete

        [Fact]
        public async void Container_Shouldnt_Exist_After_Its_Deletion()
        {
            var containerName = GetRandomContainerName();
            var blobName = GetRandomBlobName();
            var data = GenerateRandomBlobStream();

            var container = serviceClient.GetBlobContainerClient(containerName);
            await container.CreateAsync();

            await provider.DeleteContainerAsync(containerName);

            Assert.False(await ContainerExistsAsync(containerName));
        }

        [Fact]
        public void Deleting_Non_Existing_Container_Shouldnt_Throw_Exception()
        {
            var containerName = GetRandomContainerName();

            NoExceptionThrown<Exception>(async () => await provider.DeleteContainerAsync(containerName));
        }

        [Fact]
        public async void Blob_Shouldnt_Exist_After_Its_Deletion()
        {
            var containerName = GetRandomContainerName();
            var blobName = GetRandomBlobName();
            var data = GenerateRandomBlobStream();

            var container = serviceClient.GetBlobContainerClient(containerName);
            var blob = container.GetBlobClient(blobName);

            await container.CreateAsync();
            await blob.UploadAsync(data);

            await provider.DeleteBlobAsync(containerName, blobName);

            Assert.False(await BlobExistsAsync(containerName, blobName));
        }

        [Fact]
        public void Deleting_Non_Existing_Blob_Shouldnt_Throw_Execption()
        {
            var containerName = GetRandomContainerName();
            var blobName = GetRandomBlobName();

            NoExceptionThrown<Exception>(async () => await provider.DeleteBlobAsync(containerName, blobName));
        }

        #endregion
    }
}
