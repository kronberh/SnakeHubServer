using Microsoft.AspNetCore.Mvc;
using SnakeHubServer.Service;
using SnakeHubServer.Model.Game;
using SnakeHubServer.Model.Request;

namespace SnakeHubServer.Controllers
{
    [ApiController]
    [Route("api/sessions")]
    public class GamesController(GameSessionsService gameSessions) : ControllerBase
    {
        private readonly GameSessionsService _gameSessions = gameSessions;

        [HttpGet]
        public async Task<IActionResult> GetSessionsAsync()
        {
            return Ok(await _gameSessions.GetSessionsAsync());
        }

        [HttpPost]
        public async Task<IActionResult> HostGameAsync(string playerId, string modeId)
        {
            string gameId = await _gameSessions.HostGameAsync(playerId, modeId);
            return Ok(gameId);
        }

        [HttpPost("join")]
        public async Task<IActionResult> JoinGameAsync(string gameId, string playerId)
        {
            await _gameSessions.JoinGameAsync(gameId, playerId);
            return Ok();
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartGameAsync(string gameId)
        {
            await _gameSessions.StartGameAsync(gameId);
            return Ok();
        }

        [HttpGet("state")]
        public async Task<IActionResult> GetGameStateAsync(string gameId)
        {
            GameObject[][] gameState = await _gameSessions.GetGameStateAsync(gameId);
            return Ok(gameState);
        }

        [HttpGet("score")]
        public async Task<IActionResult> GetPlayerPointsAsync(string gameId, string playerId)
        {
            int score = await _gameSessions.GetPlayerScoreAsync(gameId, playerId);
            return Ok(score);
        }

        [HttpPost("action")]
        public async Task<IActionResult> PlayerActionAsync([FromQuery] string gameId, [FromQuery] string playerId, [FromBody] PlayerActionRequest actionRequest)
        {
            bool success = await _gameSessions.PlayerActionAsync(gameId, playerId, actionRequest.Action);
            return Ok(success);
        }
    }
}
