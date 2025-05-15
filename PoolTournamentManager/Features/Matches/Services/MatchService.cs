using Microsoft.EntityFrameworkCore;
using PoolTournamentManager.Features.Matches.DTOs;
using PoolTournamentManager.Features.Matches.Models;
using PoolTournamentManager.Features.Players.Models;
using PoolTournamentManager.Features.Shared.Exceptions;
using PoolTournamentManager.Shared.Infrastructure.Data;

namespace PoolTournamentManager.Features.Matches.Services
{
    public class MatchService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<MatchService> _logger;

        public MatchService(
            ApplicationDbContext dbContext,
            ILogger<MatchService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<List<MatchDto>> GetAllMatchesAsync(DateTime? startDate = null, DateTime? endDate = null, Guid? playerId = null, Guid? tournamentId = null)
        {
            var query = _dbContext.Matches
                .Include(m => m.Player1)
                .Include(m => m.Player2)
                .Include(m => m.Tournament)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(m => m.ScheduledTime >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(m => m.ScheduledTime <= endDate.Value);

            if (playerId.HasValue)
                query = query.Where(m => m.Player1Id == playerId.Value || m.Player2Id == playerId.Value);

            if (tournamentId.HasValue)
                query = query.Where(m => m.TournamentId == tournamentId.Value);

            var matches = await query.ToListAsync();

            return matches.Select(m => MapMatchToDto(m)).ToList();
        }

        public async Task<MatchDto?> GetMatchByIdAsync(Guid id)
        {
            var match = await _dbContext.Matches
                .Include(m => m.Player1)
                .Include(m => m.Player2)
                .Include(m => m.Tournament)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (match == null)
                return null;

            return MapMatchToDto(match);
        }

        public async Task<MatchDto> CreateMatchAsync(CreateMatchDto createMatchDto)
        {
            try
            {
                _logger.LogInformation("Creating match: Player1={P1}, Player2={P2}, Time={Time}",
                    createMatchDto.Player1Id, createMatchDto.Player2Id, createMatchDto.ScheduledTime);

                // Check for schedule conflicts
                if (await HasScheduleConflictAsync(createMatchDto.Player1Id, createMatchDto.Player2Id, createMatchDto.ScheduledTime))
                    throw new ScheduleConflictException(
                        "One or more players already have a match scheduled at this time",
                        createMatchDto.Player1Id,
                        createMatchDto.Player2Id,
                        createMatchDto.ScheduledTime);

                // Validate that players exist
                var player1 = await _dbContext.Players.FindAsync(createMatchDto.Player1Id);
                var player2 = await _dbContext.Players.FindAsync(createMatchDto.Player2Id);

                _logger.LogInformation("Player1 found: {Found1}, Player2 found: {Found2}",
                    player1 != null, player2 != null);

                if (player1 == null || player2 == null)
                    throw new KeyNotFoundException("One or more players were not found");

                // Validate that Player1 and Player2 are different
                if (createMatchDto.Player1Id == createMatchDto.Player2Id)
                    throw new InvalidOperationException("Player 1 and Player 2 cannot be the same player");

                var match = new Match
                {
                    ScheduledTime = createMatchDto.ScheduledTime,
                    Player1Id = createMatchDto.Player1Id,
                    Player2Id = createMatchDto.Player2Id,
                    TournamentId = createMatchDto.TournamentId,
                    Location = createMatchDto.Location,
                    Notes = createMatchDto.Notes
                };

                _logger.LogInformation("Adding match to database");
                _dbContext.Matches.Add(match);

                _logger.LogInformation("Saving changes to database");
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Match created with ID: {MatchId}", match.Id);

                var dto = await GetMatchByIdAsync(match.Id);
                if (dto == null)
                {
                    _logger.LogError("Failed to retrieve newly created match with ID: {MatchId}", match.Id);
                    throw new Exception("Failed to retrieve newly created match");
                }

                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating match: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<MatchDto?> UpdateMatchAsync(Guid id, UpdateMatchDto updateMatchDto)
        {
            var match = await _dbContext.Matches
                .Include(m => m.Player1)
                .Include(m => m.Player2)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (match == null)
                return null;

            // Update schedule if needed (check for conflicts)
            if (updateMatchDto.ScheduledTime.HasValue &&
                match.ScheduledTime != updateMatchDto.ScheduledTime.Value)
            {
                if (await HasScheduleConflictAsync(match.Player1Id, match.Player2Id, updateMatchDto.ScheduledTime.Value, id))
                    throw new ScheduleConflictException(
                        "This update would cause a schedule conflict for one or more players",
                        match.Player1Id,
                        match.Player2Id,
                        updateMatchDto.ScheduledTime.Value);

                match.ScheduledTime = updateMatchDto.ScheduledTime.Value;
            }

            // Update player assignments if needed
            bool playersChanged = false;

            if (updateMatchDto.Player1Id.HasValue && match.Player1Id != updateMatchDto.Player1Id.Value)
            {
                var player1 = await _dbContext.Players.FindAsync(updateMatchDto.Player1Id.Value);
                if (player1 == null)
                    throw new KeyNotFoundException($"Player with ID {updateMatchDto.Player1Id.Value} not found");

                match.Player1Id = updateMatchDto.Player1Id.Value;
                playersChanged = true;
            }

            if (updateMatchDto.Player2Id.HasValue && match.Player2Id != updateMatchDto.Player2Id.Value)
            {
                var player2 = await _dbContext.Players.FindAsync(updateMatchDto.Player2Id.Value);
                if (player2 == null)
                    throw new KeyNotFoundException($"Player with ID {updateMatchDto.Player2Id.Value} not found");

                match.Player2Id = updateMatchDto.Player2Id.Value;
                playersChanged = true;
            }

            // Validate that Player1 and Player2 are different
            if (match.Player1Id == match.Player2Id)
                throw new InvalidOperationException("Player 1 and Player 2 cannot be the same player");

            // Check for new conflicts if players were changed
            if (playersChanged && await HasScheduleConflictAsync(match.Player1Id, match.Player2Id, match.ScheduledTime, id))
                throw new ScheduleConflictException(
                    "This update would cause a schedule conflict for one or more players",
                    match.Player1Id,
                    match.Player2Id,
                    match.ScheduledTime);

            // Update other properties if provided
            if (updateMatchDto.EndTime.HasValue)
                match.EndTime = updateMatchDto.EndTime.Value;

            if (updateMatchDto.WinnerId.HasValue)
                match.WinnerId = updateMatchDto.WinnerId.Value;

            if (updateMatchDto.TournamentId.HasValue)
                match.TournamentId = updateMatchDto.TournamentId.Value;

            if (updateMatchDto.Location != null)
                match.Location = updateMatchDto.Location;

            if (updateMatchDto.Notes != null)
                match.Notes = updateMatchDto.Notes;

            if (updateMatchDto.Player1Score.HasValue)
                match.Player1Score = updateMatchDto.Player1Score.Value;

            if (updateMatchDto.Player2Score.HasValue)
                match.Player2Score = updateMatchDto.Player2Score.Value;

            await _dbContext.SaveChangesAsync();

            // If a winner was set, update player rankings
            if (updateMatchDto.WinnerId.HasValue)
            {
                await UpdatePlayerRankingsAsync(match);
            }

            return await GetMatchByIdAsync(id);
        }

        public async Task<bool> DeleteMatchAsync(Guid id)
        {
            var match = await _dbContext.Matches.FindAsync(id);

            if (match == null)
                return false;

            _dbContext.Matches.Remove(match);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        // Helper method to check for scheduling conflicts
        private async Task<bool> HasScheduleConflictAsync(Guid player1Id, Guid player2Id, DateTime scheduledTime, Guid? excludeMatchId = null)
        {
            // Get all matches for the specified players around the scheduled time
            // Consider a buffer before and after (e.g., 1 hour)
            var buffer = TimeSpan.FromHours(1);
            var startTimeWithBuffer = scheduledTime.Subtract(buffer);
            var endTimeWithBuffer = scheduledTime.Add(buffer);

            // Check for Player1 conflicts
            var player1Conflicts = await _dbContext.Matches
                .Where(m => (m.Player1Id == player1Id || m.Player2Id == player1Id))
                .Where(m => m.ScheduledTime >= startTimeWithBuffer && m.ScheduledTime <= endTimeWithBuffer)
                .Where(m => excludeMatchId == null || m.Id != excludeMatchId.Value)
                .AnyAsync();

            if (player1Conflicts)
                return true;

            // Check for Player2 conflicts
            var player2Conflicts = await _dbContext.Matches
                .Where(m => (m.Player1Id == player2Id || m.Player2Id == player2Id))
                .Where(m => m.ScheduledTime >= startTimeWithBuffer && m.ScheduledTime <= endTimeWithBuffer)
                .Where(m => excludeMatchId == null || m.Id != excludeMatchId.Value)
                .AnyAsync();

            return player2Conflicts;
        }

        // Helper method to update player rankings after a match
        private async Task UpdatePlayerRankingsAsync(Match match)
        {
            // Ensure players are loaded
            if (match.Player1 == null)
                match.Player1 = await _dbContext.Players.FindAsync(match.Player1Id);

            if (match.Player2 == null)
                match.Player2 = await _dbContext.Players.FindAsync(match.Player2Id);

            if (match.Player1 == null || match.Player2 == null)
                throw new InvalidOperationException("Players not found");

            if (match.WinnerId == match.Player1Id)
            {
                // Player 1 won
                match.Player1.Ranking += 10; // Increase winner's ranking
                match.Player2.Ranking = Math.Max(0, match.Player2.Ranking - 5); // Decrease loser's ranking, but not below 0
            }
            else if (match.WinnerId == match.Player2Id)
            {
                // Player 2 won
                match.Player2.Ranking += 10; // Increase winner's ranking
                match.Player1.Ranking = Math.Max(0, match.Player1.Ranking - 5); // Decrease loser's ranking, but not below 0
            }

            await _dbContext.SaveChangesAsync();
        }

        // Helper method to map Match entity to MatchDto
        private MatchDto MapMatchToDto(Match match)
        {
            return new MatchDto
            {
                Id = match.Id,
                ScheduledTime = match.ScheduledTime,
                EndTime = match.EndTime,
                WinnerId = match.WinnerId,
                TournamentId = match.TournamentId,
                TournamentName = match.Tournament?.Name,
                Player1Id = match.Player1Id,
                Player2Id = match.Player2Id,
                Player1 = match.Player1 != null ? new PlayerSummaryDto
                {
                    Id = match.Player1.Id,
                    Name = match.Player1.Name,
                    ProfilePictureUrl = match.Player1.ProfilePictureUrl
                } : null,
                Player2 = match.Player2 != null ? new PlayerSummaryDto
                {
                    Id = match.Player2.Id,
                    Name = match.Player2.Name,
                    ProfilePictureUrl = match.Player2.ProfilePictureUrl
                } : null,
                Location = match.Location,
                Notes = match.Notes,
                Player1Score = match.Player1Score,
                Player2Score = match.Player2Score
            };
        }
    }
}
