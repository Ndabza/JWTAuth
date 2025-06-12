using JWTAuth;
using JWTAuth.DTOs;
using JWTAuth.Interfaces;
using JWTAuth.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MyApp.Namespace
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private IAuthService _authSerive;

        public AuthController(IAuthService authService)
        {
            _authSerive = authService;
        }

        [HttpPost("Register")]
        public async Task<ActionResult<User>> Register(UserResponse request)
        {
            var user = await _authSerive.RegisterAsync(request);

            if (user is null)
                return BadRequest("User already exists.");

            return Ok(user);
        }

        [HttpPost("Login")]
        public async Task<ActionResult<TokenResponse>> Login(UserResponse request)
        {
            var response = await _authSerive.LoginAsync(request);

            if (response is null)
                return BadRequest("User not found");

            return Ok(response);
        }

        [HttpPost("RefreshToken")]
        public async Task<ActionResult<TokenResponse>> RefreshToken(RefreshTokenRequest request)
        {
            var response = await _authSerive.RequestRefreshTokenAsync(request);
            if(response is null || response.AccessToken == null || response.RefreshToken is null)
                return BadRequest("Invalid refresh token");
            
            return Ok(response);
        }

        [Authorize]
        [HttpGet("AuthenticatedUser")]
        public IActionResult AuthenticatedUser()
        {
            return Ok("Success");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("Admin")]
        public IActionResult AdminOnly()
        {
            return Ok("Authenticated");
        }
    }
}
