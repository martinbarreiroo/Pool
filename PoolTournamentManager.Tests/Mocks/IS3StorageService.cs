using System;
using System.Threading.Tasks;

namespace PoolTournamentManager.Tests.Mocks
{
    /// <summary>
    /// Interface to mock S3StorageService in tests
    /// </summary>
    public interface IS3StorageService
    {
        Task<(string PresignedUrl, string ObjectUrl)> GeneratePresignedUrlAsync(Guid id, string contentType);
        Task<bool> CheckBucketAccessAsync();
    }
} 