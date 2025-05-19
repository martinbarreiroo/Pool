using System;
using System.Threading.Tasks;

namespace PoolTournamentManager.Shared.Infrastructure.Storage
{
    /// <summary>
    /// Interface for storage services providing file operations
    /// </summary>
    public interface IStorageService
    {
        /// <summary>
        /// Generate a pre-signed URL for uploading a player profile picture
        /// </summary>
        /// <param name="id">The ID used to identify the file</param>
        /// <param name="contentType">The content type of the file</param>
        /// <returns>A tuple containing the presigned URL and the object URL</returns>
        Task<(string PresignedUrl, string ObjectUrl)> GeneratePresignedUrlAsync(Guid id, string contentType);

        /// <summary>
        /// Checks if the storage is accessible
        /// </summary>
        /// <returns>True if the storage is accessible, false otherwise</returns>
        Task<bool> CheckAccessAsync();
    }
}