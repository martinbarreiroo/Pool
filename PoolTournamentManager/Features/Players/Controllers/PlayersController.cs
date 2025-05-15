using Microsoft.AspNetCore.Mvc;
using PoolTournamentManager.Features.Players.DTOs;
using PoolTournamentManager.Features.Players.Services;

namespace PoolTournamentManager.Features.Players.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlayersController : ControllerBase
    {
        private readonly PlayerService _playerService;
        private readonly ILogger<PlayersController> _logger;

        public PlayersController(
            PlayerService playerService,
            ILogger<PlayersController> logger)
        {
            _playerService = playerService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PlayerDto>>> GetPlayers([FromQuery] string? searchTerm)
        {
            try
            {
                var players = await _playerService.GetAllPlayersAsync(searchTerm);
                return Ok(players);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving players");
                return StatusCode(500, "An error occurred while retrieving players");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PlayerDto>> GetPlayer(Guid id)
        {
            try
            {
                var player = await _playerService.GetPlayerByIdAsync(id);
                if (player == null)
                    return NotFound($"Player with ID {id} not found");

                return Ok(player);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving player {PlayerId}", id);
                return StatusCode(500, "An error occurred while retrieving the player");
            }
        }

        [HttpPost]
        public async Task<ActionResult<CreatePlayerResponseDto>> CreatePlayer(CreatePlayerDto createPlayerDto)
        {
            try
            {
                var response = await _playerService.CreatePlayerAsync(createPlayerDto);
                return CreatedAtAction(nameof(GetPlayer), new { id = response.Player.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating player");
                return StatusCode(500, "An error occurred while creating the player");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<PlayerDto>> UpdatePlayer(Guid id, UpdatePlayerDto updatePlayerDto)
        {
            try
            {
                var player = await _playerService.UpdatePlayerAsync(id, updatePlayerDto);
                if (player == null)
                    return NotFound($"Player with ID {id} not found");

                return Ok(player);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating player {PlayerId}", id);
                return StatusCode(500, "An error occurred while updating the player");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePlayer(Guid id)
        {
            try
            {
                var result = await _playerService.DeletePlayerAsync(id);
                if (!result)
                    return NotFound($"Player with ID {id} not found");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting player {PlayerId}", id);
                return StatusCode(500, "An error occurred while deleting the player");
            }
        }

        [HttpPost("{id}/profile-picture")]
        public async Task<ActionResult<UploadProfilePictureResponseDto>> GenerateProfilePictureUploadUrl(Guid id)
        {
            try
            {
                var result = await _playerService.GenerateProfilePictureUploadUrlAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating profile picture upload URL for player {PlayerId}", id);
                return StatusCode(500, "An error occurred while generating the profile picture upload URL");
            }
        }
    }
}
