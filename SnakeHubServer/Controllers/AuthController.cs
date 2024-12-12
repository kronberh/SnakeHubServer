using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SnakeHubServer.Model.Request;
using SnakeHubServer.Model;
using SnakeHubServer.Service;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace SnakeHubServer.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController(UserManager<User> userManager, SignInManager<User> signInManager, JwtService jwt) : ControllerBase
    {
        private readonly UserManager<User> _userManager = userManager;
        private readonly SignInManager<User> _signInManager = signInManager;
        private readonly JwtService _jwt = jwt;

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest registerRequest)
        {
            if (ModelState.IsValid)
            {
                User user = new() { UserName = registerRequest.Login };
                IdentityResult result = await _userManager.CreateAsync(user, registerRequest.Password);
                if (result.Succeeded)
                {
                    if (_userManager.Users.Count() == 1)
                    {
                        await _userManager.AddToRoleAsync(user, "admin");
                    }
                    return Ok(new { Token = await _jwt.GenerateJwtTokenAsync(user) });
                }
                if (result.Errors.Any(e => e.Code == "DuplicateUserName"))
                {
                    return Unauthorized();
                }
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return BadRequest(ModelState);
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest loginRequest)
        {
            User? user = await _userManager.FindByNameAsync(loginRequest.Login);
            if (user == null)
            {
                return Unauthorized();
            }
            SignInResult result = await _signInManager.CheckPasswordSignInAsync(user, loginRequest.Password, false);
            if (!result.Succeeded)
            {
                return Unauthorized();
            }
            return Ok(new { Token = await _jwt.GenerateJwtTokenAsync(user) });
        }
    }
}
