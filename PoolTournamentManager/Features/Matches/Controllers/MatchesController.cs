using Microsoft.AspNetCore.Mvc;
using PoolTournamentManager.Features.Matches.DTOs;
using PoolTournamentManager.Features.Matches.Services;
using PoolTournamentManager.Features.Shared.Exceptions;
namespace PoolTournamentManager.Features.Matches.Controllers
{
    /// <summary>
    /// Controller for managing pool match-related operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
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

        /// <summary>
        /// Retrieves all matches with optional filtering
        /// </summary>
        /// <param name="startDate">Optional filter for matches scheduled after this date</param>
        /// <param name="endDate">Optional filter for matches scheduled before this date</param>
        /// <param name="playerId">Optional filter for matches involving a specific player</param>
        /// <param name="tournamentId">Optional filter for matches in a specific tournament</param>
        /// <returns>A list of matches matching the filter criteria</returns>
        /// <response code="200">Returns the list of matches</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        /// <summary>
        /// Retrieves a specific match by its unique identifier
        /// </summary>
        /// <param name="id">The unique identifier of the match</param>
        /// <returns>The match details</returns>
        /// <response code="200">Returns the match</response>
        /// <response code="404">If the match is not found</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        /// <summary>
        /// Creates a new match between two players
        /// </summary>
        /// <param name="createMatchDto">The match data including player IDs and scheduled time</param>
        /// <returns>The created match details</returns>
        /// <response code="201">Returns the newly created match</response>
        /// <response code="400">If the request data is invalid</response>
        /// <response code="404">If a referenced player or tournament is not found</response>
        /// <response code="409">If there is a scheduling conflict</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        /// <summary>
        /// Updates an existing match
        /// </summary>
        /// <param name="id">The unique identifier of the match to update</param>
        /// <param name="updateMatchDto">The updated match data</param>
        /// <returns>The updated match details</returns>
        /// <response code="200">Returns the updated match</response>
        /// <response code="400">If the request data is invalid</response>
        /// <response code="404">If the match is not found</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        /// <summary>
        /// Deletes a match
        /// </summary>
        /// <param name="id">The unique identifier of the match to delete</param>
        /// <returns>No content on successful deletion</returns>
        /// <response code="204">If the match was successfully deleted</response>
        /// <response code="404">If the match is not found</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
