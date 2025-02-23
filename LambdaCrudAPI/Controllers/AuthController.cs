using LambdaCrudAPI.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace LambdaCrudAPI.Controllers
{
    [Route("api/Auth")]
    [ApiController]
    //[Authorize]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public IActionResult Login(string Email, string Password)
        {
            var token = _authService.Authenticate(Email, Password);

            if (token == null)
                return Unauthorized(new { message = "Invalid email or password" });

            return Ok(new { token });
        }

        [HttpPost("register")]
        public IActionResult Register(string Email, string Password, string FirstName, string LastName, string personType)
        {
            var result = _authService.Register(Email, Password, FirstName, LastName, personType);

            if (!result)
                return BadRequest(new { message = "Registration failed" });

            return Ok(new { message = "User registered successfully" });
        }
    }
}
