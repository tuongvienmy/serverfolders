using System.Text;
using Amazon.S3;
using Amazon.S3.Model;
using Moq;
using CloudFolders.Core;

namespace CloudFolders.Tests
{
    [TestClass]
    public class S3StorageProviderMockedTests
    {
        private Mock<IAmazonS3> _mockS3Client;
        private S3StorageProvider _s3StorageProvider;
        private const string BucketName = "test-bucket";

        [TestInitialize]
        public void Setup()
        {
            _mockS3Client = new Mock<IAmazonS3>();
            _s3StorageProvider = new S3StorageProvider(_mockS3Client.Object, BucketName);
        }

        [TestMethod]
        public async Task StoreAsync_ShouldUploadDataAndReturnStorageId()
        {
            // Arrange
            var data = Encoding.UTF8.GetBytes("test data");
            var dataStream = new MemoryStream(data); // Use a MemoryStream to avoid premature disposal
            _mockS3Client
                .Setup(client => client.PutObjectAsync(It.IsAny<PutObjectRequest>(), CancellationToken.None))
                .ReturnsAsync(new PutObjectResponse());

            // Act
            var storageId = await _s3StorageProvider.StoreAsync(data);

            // Assert
            Assert.IsNotNull(storageId);
            _mockS3Client.Verify(client => client.PutObjectAsync(It.Is<PutObjectRequest>(req =>
                req.BucketName == BucketName &&
                dataStream.Length == data.Length), default), Times.Once);
        }

        [TestMethod]
        public async Task StoreStreamAsync_ShouldUploadStreamAndReturnStorageId()
        {
            // Arrange
            var dataStream = new MemoryStream(Encoding.UTF8.GetBytes("test stream"));
            _mockS3Client
                .Setup(client => client.PutObjectAsync(It.IsAny<PutObjectRequest>(), CancellationToken.None))
                .ReturnsAsync(new PutObjectResponse());

            // Act
            var storageId = await _s3StorageProvider.StoreStreamAsync(dataStream);

            // Assert
            Assert.IsNotNull(storageId);
            _mockS3Client.Verify(client => client.PutObjectAsync(It.Is<PutObjectRequest>(req =>
                req.BucketName == BucketName &&
                req.InputStream == dataStream), default), Times.Once);
        }

        [TestMethod]
        public async Task RetrieveAsync_ShouldReturnByteArray()
        {
            // Arrange
            var storageId = "test-key";
            var responseData = Encoding.UTF8.GetBytes("retrieved data");
            var responseStream = new MemoryStream(responseData);
            _mockS3Client
                .Setup(client => client.GetObjectAsync(It.IsAny<GetObjectRequest>(), CancellationToken.None))
                .ReturnsAsync(new GetObjectResponse
                {
                    ResponseStream = new MemoryStream(Encoding.UTF8.GetBytes("retrieved data"))
                });

            // Act
            var result = await _s3StorageProvider.RetrieveAsync("S3://" + BucketName + "/" + storageId);

            // Assert
            CollectionAssert.AreEqual(responseData, result);
            _mockS3Client.Verify(client => client.GetObjectAsync(It.IsAny<GetObjectRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task RetrieveStreamAsync_ShouldReturnStream()
        {
            // Arrange
            var storageId = "test-key";
            var responseStream = new MemoryStream(Encoding.UTF8.GetBytes("retrieved stream"));
            _mockS3Client
                .Setup(client => client.GetObjectAsync(It.IsAny<GetObjectRequest>(), CancellationToken.None))
                .ReturnsAsync(new GetObjectResponse
                {
                    ResponseStream = new MemoryStream(Encoding.UTF8.GetBytes("retrieved data"))
                });

            // Act
            await using (var result = await _s3StorageProvider.RetrieveStreamAsync("S3://" + BucketName + "/" + storageId))
            {
                // Assert
                Assert.IsNotNull(result);
            }

            _mockS3Client.Verify(client => client.GetObjectAsync(It.IsAny<GetObjectRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task DeleteAsync_ShouldDeleteObject()
        {
            // Arrange
            var storageId = "test-key";
            _mockS3Client
                .Setup(client => client.DeleteObjectAsync(It.IsAny <DeleteObjectRequest>(), CancellationToken.None))
                .ReturnsAsync(new DeleteObjectResponse());

            // Act
            await _s3StorageProvider.DeleteAsync("S3://" + BucketName + "/" + storageId);

            // Assert
            _mockS3Client.Verify(client => client.DeleteObjectAsync(It.IsAny<DeleteObjectRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
