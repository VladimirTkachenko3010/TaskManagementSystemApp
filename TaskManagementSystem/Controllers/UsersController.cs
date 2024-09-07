using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace TaskManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(ILogger<UsersController> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        /// <summary>
        /// register a new user
        /// </summary>
        /// <param name="registerModel"></param>
        /// <returns></returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel registerModel)
        {
            // Check if the username or email already exists
            if (_userService.UserExists(registerModel.Username, registerModel.Email))
            {
                _logger.LogError("Username or Email already exists.");
                return BadRequest("Username or Email already exists.");
            }

            try
            {
                var user = new UserModel
                {
                    Username = registerModel.Username,
                    Email = registerModel.Email
                };

                // Password hashing and user creation happens in UserService
                // Save the user
                await _userService.CreateUserAsync(user, registerModel.Password);
                _logger.LogInformation($"User {user.Username} successfully registered.");

                return Ok(new Dictionary<string, string> { { "Message", "User registered successfully." } });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError($"Registration failed for {registerModel.Username}: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// login user and give token
        /// </summary>
        /// <param name="loginModel"></param>
        /// <returns></returns>
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel loginModel)
        {
            var user = _userService.Authenticate(loginModel.Username, loginModel.Password);

            if (user == null)
            {
                _logger.LogInformation("Invalid credentials.");
                return Unauthorized("Invalid credentials.");
            }

            var token = _userService.GenerateJwtToken(user);
            _logger.LogInformation($"User {user.Username} successfully Authenticated.");

            return Ok(new Dictionary<string, string> { { "Token", $"{token}" } });
        }
    }
}
