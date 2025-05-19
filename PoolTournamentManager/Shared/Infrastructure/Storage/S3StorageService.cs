using Amazon.S3;
using Amazon.S3.Model;
using System.Collections.Generic;
using System.Linq;

namespace PoolTournamentManager.Shared.Infrastructure.Storage
{
    public class S3StorageService : IStorageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;
        private readonly string _profilePicturePath;
        private readonly int _presignedUrlExpirationMinutes;
        private readonly List<string> _allowedImageTypes;
        private readonly ILogger<S3StorageService> _logger;

        // Add a protected property to allow derived classes to override the bucket name
        protected virtual string BucketName => _bucketName;

        public S3StorageService(
            IAmazonS3 s3Client,
            IConfiguration configuration,
            ILogger<S3StorageService> logger)
        {
            _s3Client = s3Client;

            // Try to get bucket name from configuration, then from environment variable directly as fallback
            var configBucketName = configuration?.GetValue<string>("AWS:S3:BucketName");
            var envBucketName = Environment.GetEnvironmentVariable("AWS_S3_BUCKET_NAME");

            _bucketName = !string.IsNullOrEmpty(configBucketName) && configBucketName != "${AWS_S3_BUCKET_NAME}"
                ? configBucketName
                : envBucketName ?? throw new ArgumentNullException("S3 bucket name not configured");

            logger.LogInformation("Using S3 bucket: {BucketName}", _bucketName);

            _profilePicturePath = configuration?.GetValue<string>("AWS:S3:ProfilePicturePath") ?? "players/{0}/profile";
            _presignedUrlExpirationMinutes = configuration?.GetValue<int>("AWS:S3:PresignedUrlExpirationMinutes") ?? 15;
            _allowedImageTypes = configuration?.GetSection("AWS:S3:AllowedImageTypes").Get<List<string>>() ?? new List<string> { "image/jpeg", "image/png" };
            _logger = logger;
        }

        /// <summary>
        /// Generate a pre-signed URL for uploading a player profile picture
        /// </summary>
        public virtual Task<(string PresignedUrl, string ObjectUrl)> GeneratePresignedUrlAsync(Guid playerId, string contentType)
        {
            try
            {
                // Validate content type
                if (!_allowedImageTypes.Contains(contentType))
                {
                    throw new ArgumentException($"Content type {contentType} is not allowed. Allowed types: {string.Join(", ", _allowedImageTypes)}");
                }

                // Format the path using the configured pattern
                string extension = contentType == "image/jpeg" ? ".jpg" : ".png";
                var objectKey = string.Format(_profilePicturePath, playerId) + $"-{DateTime.UtcNow.Ticks}{extension}";

                var request = new GetPreSignedUrlRequest
                {
                    BucketName = BucketName, // Use the property instead of the field
                    Key = objectKey,
                    Verb = HttpVerb.PUT,
                    Expires = DateTime.UtcNow.AddMinutes(_presignedUrlExpirationMinutes),
                    // Set ContentType property to ensure the uploaded file has the correct content type
                    ContentType = contentType
                };

                var presignedUrl = _s3Client.GetPreSignedURL(request);
                var objectUrl = $"https://{BucketName}.s3.amazonaws.com/{objectKey}";

                _logger?.LogInformation("Generated simple presigned URL for player {PlayerId}, expires in {Minutes} minutes",
                    playerId, _presignedUrlExpirationMinutes);

                return Task.FromResult((presignedUrl, objectUrl));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating presigned URL for player {PlayerId}", playerId);
                throw;
            }
        }

        /// <summary>
        /// Checks if the S3 bucket exists and is accessible
        /// </summary>
        public virtual async Task<bool> CheckBucketAccessAsync()
        {
            try
            {
                var response = await _s3Client.ListBucketsAsync();
                return response.Buckets.Any(b => b.BucketName == BucketName);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error checking access to S3 bucket {BucketName}", BucketName);
                return false;
            }
        }

        /// <summary>
        /// Implementation of IStorageService.CheckAccessAsync()
        /// </summary>
        /// <returns>True if the storage is accessible, false otherwise</returns>
        public Task<bool> CheckAccessAsync()
        {
            // Delegate to the existing implementation
            return CheckBucketAccessAsync();
        }
    }
}
