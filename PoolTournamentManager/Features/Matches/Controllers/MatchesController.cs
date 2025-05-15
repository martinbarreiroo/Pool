using Microsoft.AspNetCore.Mvc;
using PoolTournamentManager.Features.Matches.DTOs;
using PoolTournamentManager.Features.Matches.Services;
using PoolTournamentManager.Features.Shared.Exceptions;
namespace PoolTournamentManager.Features.Matches.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MatchesController : ControllerBase
    {
        private readonly MatchService _matchService;
        private readonly ILogger<MatchesController> _logger;

        public MatchesController(
            MatchService matchService,
            ILogger<MatchesController> logger)
        {
            _matchService = matchService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MatchDto>>> GetMatches(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] Guid? playerId,
            [FromQuery] Guid? tournamentId)
        {
            try
            {
                var matches = await _matchService.GetAllMatchesAsync(startDate, endDate, playerId, tournamentId);
                return Ok(matches);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving matches");
                return StatusCode(500, "An error occurred while retrieving matches");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MatchDto>> GetMatch(Guid id)
        {
            try
            {
                var match = await _matchService.GetMatchByIdAsync(id);
                if (match == null)
                    return NotFound($"Match with ID {id} not found");

                return Ok(match);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving match {MatchId}", id);
                return StatusCode(500, "An error occurred while retrieving the match");
            }
        }

        [HttpPost]
        public async Task<ActionResult<MatchDto>> CreateMatch(CreateMatchDto createMatchDto)
        {
            try
            {
                _logger.LogInformation("Match creation request received: {DTO}", System.Text.Json.JsonSerializer.Serialize(createMatchDto));

                var match = await _matchService.CreateMatchAsync(createMatchDto);
                return CreatedAtAction(nameof(GetMatch), new { id = match.Id }, match);
            }
            catch (ScheduleConflictException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while creating match");
                return Conflict(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Resource not found while creating match");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error creating match: {Message}, {StackTrace}", ex.Message, ex.StackTrace);
                return StatusCode(500, $"An error occurred while creating the match: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<MatchDto>> UpdateMatch(Guid id, UpdateMatchDto updateMatchDto)
        {
            try
            {
                var match = await _matchService.UpdateMatchAsync(id, updateMatchDto);
                if (match == null)
                    return NotFound($"Match with ID {id} not found");

                return Ok(match);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while updating match {MatchId}", id);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating match {MatchId}", id);
                return StatusCode(500, "An error occurred while updating the match");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMatch(Guid id)
        {
            try
            {
                var result = await _matchService.DeleteMatchAsync(id);
                if (!result)
                    return NotFound($"Match with ID {id} not found");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting match {MatchId}", id);
                return StatusCode(500, "An error occurred while deleting the match");
            }
        }
    }
}
