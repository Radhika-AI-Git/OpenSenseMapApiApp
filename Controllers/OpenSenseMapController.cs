using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenSenseMapApiService.Models;
using OpenSenseMapApiService.Services;
using System.Net.Http.Headers;
using System.Net.Http;



namespace OpenSenseMapProxyApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OpenSenseMapController : ControllerBase
    {
        private readonly IOpenSenseMapService _openSenseMapService;

        public OpenSenseMapController(IOpenSenseMapService openSenseMapService)
        {
            _openSenseMapService = openSenseMapService;
        }

        [HttpPost("registerUser")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterUserRequestDto request)
        {
            var user = new OpenSenseMapUser
            {
                name = request.Name,
                email = request.Email,
                password = request.Password
            };

            var result = await _openSenseMapService.RegisterUserAsync(user);
            return Ok(result);
        }




        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
        {
            try
            {
                var result = await _openSenseMapService.LoginUserAsync(loginRequest);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPost("sensebox")]
        
        public async Task<IActionResult> CreateSenseBox([FromBody] NewSenseBoxRequestDto senseBoxRequest,[FromHeader(Name = "Authorization")] string authorizationHeader)
        {
            try
            {
                if (!authorizationHeader.StartsWith("Bearer "))
                    return Unauthorized("Missing or invalid Authorization header.");

                var token = authorizationHeader.Substring("Bearer ".Length).Trim();

                var result = await _openSenseMapService.CreateSenseBoxAsync(senseBoxRequest, token);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("sensebox/{id}")]
        public async Task<IActionResult> GetSenseBoxById(string id)
        {
            try
            {
                var box = await _openSenseMapService.GetSenseBoxByIdAsync(id);
                return Ok(box);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromHeader(Name = "Authorization")] string authorizationHeader)
        {
            try
            {
                if (!authorizationHeader.StartsWith("Bearer "))
                    return Unauthorized("Missing or invalid Authorization header.");

                var token = authorizationHeader.Substring("Bearer ".Length).Trim();

                var result = await _openSenseMapService.LogoutAsync(token);
                return Ok(new { message = "User logged out successfully", success = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

    }
}