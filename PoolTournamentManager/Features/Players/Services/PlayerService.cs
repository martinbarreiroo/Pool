using Microsoft.EntityFrameworkCore;
using PoolTournamentManager.Features.Players.DTOs;
using PoolTournamentManager.Features.Players.Models;
using PoolTournamentManager.Shared.Infrastructure.Data;
using PoolTournamentManager.Shared.Infrastructure.Storage;

namespace PoolTournamentManager.Features.Players.Services
{
    public class PlayerService
    {
        protected readonly ApplicationDbContext _dbContext;
        private readonly IStorageService? _storageService;
        public PlayerService(
            ApplicationDbContext dbContext,
            IStorageService? storageService)
        {
            _dbContext = dbContext;
            _storageService = storageService;
        }

        public virtual async Task<List<PlayerDto>> GetAllPlayersAsync(string? searchTerm = null)
        {
            var query = _dbContext.Players
                .Include(p => p.MatchesAsPlayer1)
                .Include(p => p.MatchesAsPlayer2)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(searchTerm) ||
                                          p.Email.ToLower().Contains(searchTerm));
            }

            return await query
                .Select(p => new PlayerDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Email = p.Email,
                    ProfilePictureUrl = p.ProfilePictureUrl,
                    PreferredCue = p.PreferredCue,
                    Ranking = p.Ranking,
                    MatchCount = p.MatchesAsPlayer1.Count + p.MatchesAsPlayer2.Count
                })
                .ToListAsync();
        }

        public virtual async Task<PlayerDto?> GetPlayerByIdAsync(Guid id)
        {
            var player = await _dbContext.Players
                .Include(p => p.MatchesAsPlayer1)
                .Include(p => p.MatchesAsPlayer2)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (player == null)
                return null;

            return new PlayerDto
            {
                Id = player.Id,
                Name = player.Name,
                Email = player.Email,
                ProfilePictureUrl = player.ProfilePictureUrl,
                PreferredCue = player.PreferredCue,
                Ranking = player.Ranking,
                MatchCount = player.MatchesAsPlayer1.Count + player.MatchesAsPlayer2.Count
            };
        }

        public virtual async Task<CreatePlayerResponseDto> CreatePlayerAsync(CreatePlayerDto createPlayerDto)
        {
            // Create player with a placeholder profile picture URL
            var player = new Player
            {
                Name = createPlayerDto.Name,
                Email = createPlayerDto.Email,
                ProfilePictureUrl = string.Empty, // Placeholder, will be updated with actual URL
                PreferredCue = createPlayerDto.PreferredCue,
                // Initialize with default value
                Ranking = 0
            };

            _dbContext.Players.Add(player);
            await _dbContext.SaveChangesAsync();

            // Generate presigned URL for profile picture upload
            var (presignedUrl, objectUrl) = _storageService != null
                ? await _storageService.GeneratePresignedUrlAsync(
                    player.Id,
                    createPlayerDto.ContentType // Use the content type specified by the client
                  )
                : (string.Empty, string.Empty);

            // Update player with the profile picture URL
            player.ProfilePictureUrl = objectUrl;
            await _dbContext.SaveChangesAsync();

            // Create response with player data and presigned URL
            return new CreatePlayerResponseDto
            {
                Player = new PlayerDto
                {
                    Id = player.Id,
                    Name = player.Name,
                    Email = player.Email,
                    ProfilePictureUrl = player.ProfilePictureUrl,
                    PreferredCue = player.PreferredCue,
                    Ranking = player.Ranking,
                    MatchCount = 0
                },
                PresignedUrl = presignedUrl
            };
        }

        public virtual async Task<PlayerDto?> UpdatePlayerAsync(Guid id, UpdatePlayerDto updatePlayerDto)
        {
            var player = await _dbContext.Players
                .Include(p => p.MatchesAsPlayer1)
                .Include(p => p.MatchesAsPlayer2)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (player == null)
                return null;

            // Update only non-null properties
            if (updatePlayerDto.Name != null)
                player.Name = updatePlayerDto.Name;

            if (updatePlayerDto.Email != null)
                player.Email = updatePlayerDto.Email;

            if (updatePlayerDto.ProfilePictureUrl != null)
                player.ProfilePictureUrl = updatePlayerDto.ProfilePictureUrl;

            if (updatePlayerDto.PreferredCue != null)
                player.PreferredCue = updatePlayerDto.PreferredCue;

            await _dbContext.SaveChangesAsync();

            return new PlayerDto
            {
                Id = player.Id,
                Name = player.Name,
                Email = player.Email,
                ProfilePictureUrl = player.ProfilePictureUrl,
                PreferredCue = player.PreferredCue,
                Ranking = player.Ranking,
                MatchCount = player.MatchesAsPlayer1.Count + player.MatchesAsPlayer2.Count
            };
        }

        public virtual async Task<bool> DeletePlayerAsync(Guid id)
        {
            var player = await _dbContext.Players.FindAsync(id);

            if (player == null)
                return false;

            _dbContext.Players.Remove(player);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public virtual async Task<UploadProfilePictureResponseDto> GenerateProfilePictureUploadUrlAsync(Guid playerId)
        {
            // Check if player exists
            var player = await _dbContext.Players.FindAsync(playerId);
            if (player == null)
                throw new KeyNotFoundException($"Player with ID {playerId} not found");

            // Generate presigned URL
            var (presignedUrl, objectUrl) = _storageService != null
                ? await _storageService.GeneratePresignedUrlAsync(
                    playerId,
                    "image/jpeg"
                  )
                : (string.Empty, string.Empty);

            // Update player with the new picture URL
            player.ProfilePictureUrl = objectUrl;
            await _dbContext.SaveChangesAsync();

            return new UploadProfilePictureResponseDto
            {
                PresignedUrl = presignedUrl,
                ProfilePictureUrl = objectUrl
            };
        }
    }
}
