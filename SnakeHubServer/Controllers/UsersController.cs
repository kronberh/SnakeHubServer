using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SnakeHubServer.Data;
using SnakeHubServer.Service;

namespace SnakeHubServer.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController(ApplicationContext context, GameSessionsService gameSessions) : ControllerBase
    {
        private readonly ApplicationContext _context = context;
        private readonly GameSessionsService _gameSessions = gameSessions;

        [HttpGet]
        public async Task<IActionResult> GetUsersAsync()
        {
            return Ok(await _context.Users.ToListAsync());
        }

        [HttpGet("session")]
        public async Task<IActionResult> GetPlayerGameAsync(string userId)
        {
            return Ok(await _gameSessions.GetPlayerGameAsync(userId));
        }
    }
}
