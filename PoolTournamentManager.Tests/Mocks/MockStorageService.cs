using System;
using System.Threading.Tasks;
using PoolTournamentManager.Shared.Infrastructure.Storage;

namespace PoolTournamentManager.Tests.Mocks
{
    /// <summary>
    /// A mock implementation of IStorageService for testing
    /// </summary>
    public class MockStorageService : IStorageService
    {
        private readonly string _defaultPresignedUrl;
        private readonly string _defaultObjectUrl;
        private readonly bool _isStorageAccessible;

        public MockStorageService(
            string defaultPresignedUrl = "https://test-presigned-url.com",
            string defaultObjectUrl = "https://test-bucket.s3.amazonaws.com/test-image.jpg",
            bool isStorageAccessible = true)
        {
            _defaultPresignedUrl = defaultPresignedUrl;
            _defaultObjectUrl = defaultObjectUrl;
            _isStorageAccessible = isStorageAccessible;
        }

        public Task<(string PresignedUrl, string ObjectUrl)> GeneratePresignedUrlAsync(Guid id, string contentType)
        {
            // Validate the content type
            if (contentType != "image/jpeg" && contentType != "image/png")
            {
                throw new ArgumentException($"Content type {contentType} is not allowed. Allowed types: image/jpeg, image/png");
            }

            return Task.FromResult((_defaultPresignedUrl, _defaultObjectUrl));
        }

        public Task<bool> CheckAccessAsync()
        {
            return Task.FromResult(_isStorageAccessible);
        }
    }
}
