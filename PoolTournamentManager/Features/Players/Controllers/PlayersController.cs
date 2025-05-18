using Microsoft.AspNetCore.Mvc;
using PoolTournamentManager.Features.Players.DTOs;
using PoolTournamentManager.Features.Players.Services;

namespace PoolTournamentManager.Features.Players.Controllers
{
    /// <summary>
    /// Controller for managing player-related operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
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

        /// <summary>
        /// Retrieves all players, optionally filtered by search term
        /// </summary>
        /// <param name="searchTerm">Optional search term to filter players by name</param>
        /// <returns>A list of players matching the criteria</returns>
        /// <response code="200">Returns the list of players</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        /// <summary>
        /// Retrieves a specific player by their unique identifier
        /// </summary>
        /// <param name="id">The unique identifier of the player</param>
        /// <returns>The player details</returns>
        /// <response code="200">Returns the player</response>
        /// <response code="404">If the player is not found</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        /// <summary>
        /// Creates a new player
        /// </summary>
        /// <param name="createPlayerDto">The player data</param>
        /// <returns>The created player with upload URL information</returns>
        /// <response code="201">Returns the newly created player</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        /// <summary>
        /// Updates an existing player
        /// </summary>
        /// <param name="id">The unique identifier of the player to update</param>
        /// <param name="updatePlayerDto">The updated player data</param>
        /// <returns>The updated player details</returns>
        /// <response code="200">Returns the updated player</response>
        /// <response code="404">If the player is not found</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        /// <summary>
        /// Deletes a player
        /// </summary>
        /// <param name="id">The unique identifier of the player to delete</param>
        /// <returns>No content on successful deletion</returns>
        /// <response code="204">If the player was successfully deleted</response>
        /// <response code="404">If the player is not found</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        /// <summary>
        /// Generates a pre-signed URL for uploading a player's profile picture
        /// </summary>
        /// <param name="id">The unique identifier of the player</param>
        /// <returns>Upload URL information for the profile picture</returns>
        /// <response code="200">Returns the upload URL information</response>
        /// <response code="404">If the player is not found</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpPost("{id}/profile-picture")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
