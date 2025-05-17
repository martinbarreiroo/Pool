using Microsoft.AspNetCore.Mvc;
using PoolTournamentManager.Features.Tournaments.DTOs;
using PoolTournamentManager.Features.Tournaments.Services;

namespace PoolTournamentManager.Features.Tournaments.Controllers
{
    /// <summary>
    /// Controller for managing tournament-related operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TournamentsController : ControllerBase
    {
        private readonly TournamentService _tournamentService;
        private readonly ILogger<TournamentsController> _logger;

        public TournamentsController(
            TournamentService tournamentService,
            ILogger<TournamentsController> logger)
        {
            _tournamentService = tournamentService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all tournaments, optionally filtered by active status
        /// </summary>
        /// <param name="isActive">Optional filter for active/inactive tournaments</param>
        /// <returns>A list of tournaments matching the criteria</returns>
        /// <response code="200">Returns the list of tournaments</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<TournamentDto>>> GetTournaments([FromQuery] bool? isActive)
        {
            try
            {
                var tournaments = await _tournamentService.GetAllTournamentsAsync(isActive);
                return Ok(tournaments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tournaments");
                return StatusCode(500, "An error occurred while retrieving tournaments");
            }
        }

        /// <summary>
        /// Retrieves a specific tournament by its unique identifier
        /// </summary>
        /// <param name="id">The unique identifier of the tournament</param>
        /// <returns>The tournament details</returns>
        /// <response code="200">Returns the tournament</response>
        /// <response code="404">If the tournament is not found</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TournamentDto>> GetTournament(Guid id)
        {
            try
            {
                var tournament = await _tournamentService.GetTournamentByIdAsync(id);
                if (tournament == null)
                    return NotFound($"Tournament with ID {id} not found");

                return Ok(tournament);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tournament {TournamentId}", id);
                return StatusCode(500, "An error occurred while retrieving the tournament");
            }
        }

        /// <summary>
        /// Creates a new tournament
        /// </summary>
        /// <param name="createTournamentDto">The tournament data</param>
        /// <returns>The created tournament</returns>
        /// <response code="201">Returns the newly created tournament</response>
        /// <response code="400">If the tournament data is invalid</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TournamentDto>> CreateTournament(CreateTournamentDto createTournamentDto)
        {
            try
            {
                var tournament = await _tournamentService.CreateTournamentAsync(createTournamentDto);
                return CreatedAtAction(nameof(GetTournament), new { id = tournament.Id }, tournament);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tournament");
                return StatusCode(500, "An error occurred while creating the tournament");
            }
        }

        /// <summary>
        /// Updates an existing tournament
        /// </summary>
        /// <param name="id">The unique identifier of the tournament to update</param>
        /// <param name="updateTournamentDto">The updated tournament data</param>
        /// <returns>The updated tournament details</returns>
        /// <response code="200">Returns the updated tournament</response>
        /// <response code="404">If the tournament is not found</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TournamentDto>> UpdateTournament(Guid id, UpdateTournamentDto updateTournamentDto)
        {
            try
            {
                var tournament = await _tournamentService.UpdateTournamentAsync(id, updateTournamentDto);
                if (tournament == null)
                    return NotFound($"Tournament with ID {id} not found");

                return Ok(tournament);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tournament {TournamentId}", id);
                return StatusCode(500, "An error occurred while updating the tournament");
            }
        }

        /// <summary>
        /// Deletes a tournament
        /// </summary>
        /// <param name="id">The unique identifier of the tournament to delete</param>
        /// <returns>No content on successful deletion</returns>
        /// <response code="204">If the tournament was successfully deleted</response>
        /// <response code="400">If the tournament cannot be deleted (e.g., has active matches)</response>
        /// <response code="404">If the tournament is not found</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteTournament(Guid id)
        {
            try
            {
                var result = await _tournamentService.DeleteTournamentAsync(id);
                if (!result)
                    return NotFound($"Tournament with ID {id} not found");

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while deleting tournament {TournamentId}", id);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting tournament {TournamentId}", id);
                return StatusCode(500, "An error occurred while deleting the tournament");
            }
        }
    }
}
