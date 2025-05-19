using Microsoft.EntityFrameworkCore;
using PoolTournamentManager.Features.Tournaments.DTOs;
using PoolTournamentManager.Features.Tournaments.Models;
using PoolTournamentManager.Shared.Infrastructure.Data;

namespace PoolTournamentManager.Features.Tournaments.Services
{
    public class TournamentService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<TournamentService> _logger;

        public TournamentService(
            ApplicationDbContext dbContext,
            ILogger<TournamentService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<List<TournamentDto>> GetAllTournamentsAsync(bool? isActive = null)
        {
            var query = _dbContext.Tournaments
                .Include(t => t.Matches)
                .AsQueryable();

            if (isActive.HasValue)
                query = query.Where(t => t.IsActive == isActive.Value);

            return await query.Select(t => new TournamentDto
            {
                Id = t.Id,
                Name = t.Name,
                StartDate = t.StartDate,
                EndDate = t.EndDate,
                Location = t.Location,
                Description = t.Description,
                IsActive = t.IsActive,
                MatchCount = t.Matches.Count
            }).ToListAsync();
        }

        public async Task<TournamentDto?> GetTournamentByIdAsync(Guid id)
        {
            var tournament = await _dbContext.Tournaments
                .Include(t => t.Matches)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tournament == null)
                return null;

            return new TournamentDto
            {
                Id = tournament.Id,
                Name = tournament.Name,
                StartDate = tournament.StartDate,
                EndDate = tournament.EndDate,
                Location = tournament.Location,
                Description = tournament.Description,
                IsActive = tournament.IsActive,
                MatchCount = tournament.Matches.Count
            };
        }

        public async Task<TournamentDto> CreateTournamentAsync(CreateTournamentDto createTournamentDto)
        {
            var tournament = new Tournament
            {
                Name = createTournamentDto.Name,
                StartDate = createTournamentDto.StartDate,
                EndDate = createTournamentDto.EndDate,
                Location = createTournamentDto.Location,
                Description = createTournamentDto.Description,
                IsActive = true // New tournaments are active by default
            };

            _dbContext.Tournaments.Add(tournament);
            await _dbContext.SaveChangesAsync();

            return new TournamentDto
            {
                Id = tournament.Id,
                Name = tournament.Name,
                StartDate = tournament.StartDate,
                EndDate = tournament.EndDate,
                Location = tournament.Location,
                Description = tournament.Description,
                IsActive = tournament.IsActive,
                MatchCount = 0 // New tournament has no matches yet
            };
        }

        public async Task<TournamentDto?> UpdateTournamentAsync(Guid id, UpdateTournamentDto updateTournamentDto)
        {
            var tournament = await _dbContext.Tournaments.FindAsync(id);

            if (tournament == null)
                return null;

            // Update only the properties that are provided
            if (updateTournamentDto.Name != null)
                tournament.Name = updateTournamentDto.Name;

            if (updateTournamentDto.StartDate.HasValue)
                tournament.StartDate = updateTournamentDto.StartDate.Value;

            if (updateTournamentDto.EndDate.HasValue)
                tournament.EndDate = updateTournamentDto.EndDate;

            if (updateTournamentDto.Location != null)
                tournament.Location = updateTournamentDto.Location;

            if (updateTournamentDto.Description != null)
                tournament.Description = updateTournamentDto.Description;

            if (updateTournamentDto.IsActive.HasValue)
                tournament.IsActive = updateTournamentDto.IsActive.Value;

            await _dbContext.SaveChangesAsync();

            return await GetTournamentByIdAsync(id);
        }

        public async Task<bool> DeleteTournamentAsync(Guid id)
        {
            var tournament = await _dbContext.Tournaments.FindAsync(id);

            if (tournament == null)
                return false;

            // Optional: Check if the tournament has matches and handle accordingly
            var hasMatches = await _dbContext.Matches.AnyAsync(m => m.TournamentId == id);
            if (hasMatches)
            {
                // Option 1: Prevent deletion
                // throw new InvalidOperationException("Cannot delete tournament with existing matches");

                // Option 2: Set TournamentId to null in related matches
                var relatedMatches = await _dbContext.Matches.Where(m => m.TournamentId == id).ToListAsync();
                foreach (var match in relatedMatches)
                {
                    match.TournamentId = null;
                }
            }

            _dbContext.Tournaments.Remove(tournament);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Gets the current match count for a tournament
        /// </summary>
        /// <param name="tournamentId">The tournament ID</param>
        /// <returns>The number of matches in the tournament, or zero if the tournament doesn't exist</returns>
        public async Task<int> GetTournamentMatchCountAsync(Guid tournamentId)
        {
            return await _dbContext.Matches
                .Where(m => m.TournamentId == tournamentId)
                .CountAsync();
        }

        /// <summary>
        /// Updates the tournament's match collection by ensuring all matches with this tournament ID
        /// are properly included in the tournament's Matches collection
        /// </summary>
        /// <param name="tournamentId">The tournament ID</param>
        /// <returns>The updated match count, or -1 if the tournament doesn't exist</returns>
        public async Task<int> RefreshTournamentMatchesAsync(Guid tournamentId)
        {
            var tournament = await _dbContext.Tournaments
                .Include(t => t.Matches)
                .FirstOrDefaultAsync(t => t.Id == tournamentId);

            if (tournament == null)
                return -1;

            // Find all matches that should belong to this tournament
            var tournamentMatches = await _dbContext.Matches
                .Where(m => m.TournamentId == tournamentId)
                .ToListAsync();

            // Ensure each match is in the tournament's collection
            bool collectionChanged = false;
            foreach (var match in tournamentMatches)
            {
                if (!tournament.Matches.Any(m => m.Id == match.Id))
                {
                    tournament.Matches.Add(match);
                    collectionChanged = true;
                }
            }

            // Remove any matches from collection that don't belong
            var matchesToRemove = tournament.Matches
                .Where(m => m.TournamentId != tournamentId)
                .ToList();

            foreach (var match in matchesToRemove)
            {
                tournament.Matches.Remove(match);
                collectionChanged = true;
            }

            if (collectionChanged)
            {
                await _dbContext.SaveChangesAsync();
            }

            return tournament.Matches.Count;
        }
    }
}
