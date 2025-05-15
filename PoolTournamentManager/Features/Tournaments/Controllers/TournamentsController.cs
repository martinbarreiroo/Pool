using Microsoft.AspNetCore.Mvc;
using PoolTournamentManager.Features.Tournaments.DTOs;
using PoolTournamentManager.Features.Tournaments.Services;

namespace PoolTournamentManager.Features.Tournaments.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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

        [HttpGet]
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

        [HttpGet("{id}")]
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

        [HttpPost]
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

        [HttpPut("{id}")]
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

        [HttpDelete("{id}")]
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
