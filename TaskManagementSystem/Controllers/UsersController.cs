using Application.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace TaskManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel registerModel)
        {
            // Check if the username or email already exists
            if (_userService.UserExists(registerModel.Username, registerModel.Email))
                return BadRequest("Username or Email already exists.");

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

                return Ok(new { Message = "User registered successfully." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message); 
            }
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel loginModel)
        {
            var user = _userService.Authenticate(loginModel.Username, loginModel.Password);

            if (user == null)
                return Unauthorized("Invalid credentials.");

            var token = _userService.GenerateJwtToken(user);

            return Ok(new { Token = token });
        }
    }
}
