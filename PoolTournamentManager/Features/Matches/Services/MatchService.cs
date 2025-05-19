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

                // Validate that tournament exists if provided and load it with its matches
                if (createMatchDto.TournamentId.HasValue)
                {
                    var tournament = await _dbContext.Tournaments
                        .Include(t => t.Matches)
                        .FirstOrDefaultAsync(t => t.Id == createMatchDto.TournamentId.Value);

                    if (tournament == null)
                        throw new KeyNotFoundException($"Tournament with ID {createMatchDto.TournamentId.Value} not found");

                    _logger.LogInformation("Tournament found: {TournamentId}, {TournamentName}, Current match count: {MatchCount}",
                        tournament.Id, tournament.Name, tournament.Matches.Count);
                }

                // Get tournament location if needed
                string? matchLocation = createMatchDto.Location;
                if (createMatchDto.TournamentId.HasValue && string.IsNullOrEmpty(matchLocation))
                {
                    var tournament = await _dbContext.Tournaments
                        .Include(t => t.Matches)
                        .FirstOrDefaultAsync(t => t.Id == createMatchDto.TournamentId.Value);

                    if (tournament != null && !string.IsNullOrEmpty(tournament.Location))
                    {
                        _logger.LogInformation("Using tournament location for match: {Location}", tournament.Location);
                        matchLocation = tournament.Location;
                    }
                }

                var match = new Match
                {
                    ScheduledTime = createMatchDto.ScheduledTime,
                    Player1Id = createMatchDto.Player1Id,
                    Player2Id = createMatchDto.Player2Id,
                    TournamentId = createMatchDto.TournamentId,
                    Location = matchLocation,
                };

                _logger.LogInformation("Adding match to database");
                _dbContext.Matches.Add(match);

                // If this match belongs to a tournament, make sure it's properly added to the tournament's collection
                if (createMatchDto.TournamentId.HasValue)
                {
                    var tournament = await _dbContext.Tournaments
                        .Include(t => t.Matches)
                        .FirstOrDefaultAsync(t => t.Id == createMatchDto.TournamentId.Value);

                    if (tournament != null)
                    {
                        _logger.LogInformation("Explicitly adding match to tournament's match collection");
                        tournament.Matches.Add(match);
                    }
                }

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

            // Handle tournament association changes
            if (updateMatchDto.TournamentId.HasValue)
            {
                var oldTournamentId = match.TournamentId;
                var newTournamentId = updateMatchDto.TournamentId.Value;

                // If tournament is changing
                if (oldTournamentId != newTournamentId)
                {
                    // Remove from old tournament if applicable
                    if (oldTournamentId.HasValue)
                    {
                        var oldTournament = await _dbContext.Tournaments
                            .Include(t => t.Matches)
                            .FirstOrDefaultAsync(t => t.Id == oldTournamentId.Value);

                        if (oldTournament != null)
                        {
                            _logger.LogInformation("Removing match from previous tournament: {TournamentId}", oldTournamentId);
                            var matchInCollection = oldTournament.Matches.FirstOrDefault(m => m.Id == match.Id);
                            if (matchInCollection != null)
                            {
                                oldTournament.Matches.Remove(matchInCollection);
                            }
                        }
                    }

                    // Add to new tournament
                    var newTournament = await _dbContext.Tournaments
                        .Include(t => t.Matches)
                        .FirstOrDefaultAsync(t => t.Id == newTournamentId);

                    if (newTournament == null)
                        throw new KeyNotFoundException($"Tournament with ID {newTournamentId} not found");

                    _logger.LogInformation("Adding match to new tournament: {TournamentId}, {TournamentName}, Current match count: {MatchCount}",
                        newTournament.Id, newTournament.Name, newTournament.Matches.Count);

                    // If location wasn't explicitly provided and match location is empty, use tournament location
                    if (updateMatchDto.Location == null && string.IsNullOrEmpty(match.Location) &&
                        !string.IsNullOrEmpty(newTournament.Location))
                    {
                        _logger.LogInformation("Using tournament location for match: {Location}", newTournament.Location);
                        match.Location = newTournament.Location;
                    }

                    newTournament.Matches.Add(match);
                    match.TournamentId = newTournamentId;
                }
            }

            if (updateMatchDto.Location != null)
                match.Location = updateMatchDto.Location;


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

            // Handle tournament association if match belongs to a tournament
            if (match.TournamentId.HasValue)
            {
                var tournament = await _dbContext.Tournaments
                    .Include(t => t.Matches)
                    .FirstOrDefaultAsync(t => t.Id == match.TournamentId.Value);

                if (tournament != null)
                {
                    _logger.LogInformation("Removing match from tournament {TournamentId} before deletion", match.TournamentId);
                    var matchInCollection = tournament.Matches.FirstOrDefault(m => m.Id == match.Id);
                    if (matchInCollection != null)
                    {
                        tournament.Matches.Remove(matchInCollection);
                    }
                }
            }

            _dbContext.Matches.Remove(match);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        // Helper method to check for scheduling conflicts
        private async Task<bool> HasScheduleConflictAsync(Guid player1Id, Guid player2Id, DateTime scheduledTime, Guid? excludeMatchId = null)
        {
            // Default buffer - used for matches that don't have an end time yet
            var defaultBuffer = TimeSpan.FromMinutes(30);

            // Load all potentially conflicting matches for both players
            var allMatches = await _dbContext.Matches
                .Where(m => m.Player1Id == player1Id || m.Player2Id == player1Id ||
                            m.Player1Id == player2Id || m.Player2Id == player2Id)
                .Where(m => excludeMatchId == null || m.Id != excludeMatchId.Value)
                .ToListAsync();

            // Separate the matches for each player (a match could involve both players)
            var player1Matches = allMatches.Where(m => m.Player1Id == player1Id || m.Player2Id == player1Id).ToList();
            var player2Matches = allMatches.Where(m => m.Player1Id == player2Id || m.Player2Id == player2Id)
                                         .Where(m => !player1Matches.Any(p1m => p1m.Id == m.Id)) // Filter out matches that involve both players
                                         .ToList();

            _logger.LogDebug("Found {Player1MatchCount} matches for Player1 and {Player2MatchCount} unique matches for Player2",
                player1Matches.Count, player2Matches.Count);

            // Check for conflicts with Player1's matches
            foreach (var match in player1Matches)
            {
                if (IsTimeConflicting(match, scheduledTime, defaultBuffer))
                {
                    _logger.LogDebug("Found conflict with match {MatchId} for Player1", match.Id);
                    return true;
                }
            }

            // Check for conflicts with Player2's matches
            foreach (var match in player2Matches)
            {
                if (IsTimeConflicting(match, scheduledTime, defaultBuffer))
                {
                    _logger.LogDebug("Found conflict with match {MatchId} for Player2", match.Id);
                    return true;
                }
            }

            return false;
        }

        // Helper to determine if a match conflicts with a proposed time
        private bool IsTimeConflicting(Match match, DateTime proposedTime, TimeSpan defaultBuffer)
        {
            // Start time is always the scheduled time
            var matchStartTime = match.ScheduledTime;

            // Special handling for "future" matches (scheduled in the future)
            var now = DateTime.UtcNow;
            bool isMatchInFuture = matchStartTime > now;

            // End time calculation depends on match status
            DateTime matchEndTime;

            // If match has an actual end time, use it
            if (match.EndTime.HasValue)
            {
                matchEndTime = match.EndTime.Value;
                _logger.LogDebug("Using actual end time for match {MatchId}: {EndTime}", match.Id, matchEndTime);
            }
            // If match is in the past with no end time (ongoing or improperly closed)
            else if (!isMatchInFuture)
            {
                // For past matches with no end time, assume it ended 45 mins after start
                matchEndTime = matchStartTime.AddMinutes(45);
                _logger.LogDebug("Past match {MatchId} has no EndTime, assuming it ended at {AssumedEndTime}",
                    match.Id, matchEndTime);
            }
            // For future matches, add the buffer
            else
            {
                matchEndTime = matchStartTime.Add(defaultBuffer);
                _logger.LogDebug("Future match {MatchId} - using scheduled time plus buffer: {MatchEnd}",
                    match.Id, matchEndTime);
            }

            // For the proposed match, we'll apply buffer differently based on match status
            DateTime proposedStartTime;
            DateTime proposedEndTime;

            // If the existing match has ended (has EndTime or is in past), use minimal buffer 
            if (match.EndTime.HasValue || !isMatchInFuture)
            {
                // For new match after completed match, just apply a small buffer before (prep time) 
                proposedStartTime = proposedTime;
                proposedEndTime = proposedTime.Add(defaultBuffer);

                _logger.LogDebug("Match {MatchId} is completed, using no prep buffer", match.Id);
            }
            else
            {
                // For conflict with future match, we need reasonable buffers on both sides
                proposedStartTime = proposedTime.Subtract(TimeSpan.FromMinutes(15));
                proposedEndTime = proposedTime.Add(defaultBuffer);

                _logger.LogDebug("Checking conflict with future match {MatchId}, using 15min prep buffer", match.Id);
            }

            // Check for overlap
            // (proposedStart < matchEnd) && (proposedEnd > matchStart) means there's an overlap
            bool hasConflict = proposedStartTime < matchEndTime && proposedEndTime > matchStartTime;

            if (hasConflict)
            {
                _logger.LogDebug("Conflict detected: Existing match {MatchId} ({ExistingStart} to {ExistingEnd}) overlaps with proposed time ({ProposedStart} to {ProposedEnd})",
                    match.Id, matchStartTime, matchEndTime, proposedStartTime, proposedEndTime);
            }

            return hasConflict;
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
                match.Player1.Ranking += 1; // Increase winner's ranking
                match.Player2.Ranking = Math.Max(0, match.Player2.Ranking - 1); // Decrease loser's ranking, but not below 0
            }
            else if (match.WinnerId == match.Player2Id)
            {
                // Player 2 won
                match.Player2.Ranking += 1; // Increase winner's ranking
                match.Player1.Ranking = Math.Max(0, match.Player1.Ranking - 1); // Decrease loser's ranking, but not below 0
            }

            await _dbContext.SaveChangesAsync();
        }

        // Helper method to map Match entity to MatchDto
        private MatchDto MapMatchToDto(Match match)
        {
            // Log tournament information if available
            if (match.TournamentId.HasValue && match.Tournament != null)
            {
                _logger.LogDebug("Match {MatchId} is part of tournament {TournamentId}: {TournamentName}",
                    match.Id, match.TournamentId, match.Tournament.Name);
            }

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
                Location = match.Location
            };
        }
    }
}
