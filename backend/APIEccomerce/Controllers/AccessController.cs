using APIEccomerce.Models;
using APIEccomerce.Models.DTOs;
using APIEccomerce.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APIEccomerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AccessController : ControllerBase
    {
        private readonly IAccessService _service;

        public AccessController(IAccessService service)
        {
            _service = service;
        }

        // =========================
        // REGISTER
        // =========================
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserDto dto)
        {
            try
            {
                var message = await _service.Register(dto);

                return Ok(new Response<string>
                {
                    Value = message,
                    IsSuccess = true
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new Response<string>
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
        }

        // =========================
        // LOGIN
        // =========================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await _service.Login(dto);

            if (result == null)
            {
                return Unauthorized(new Response<string>
                {
                    IsSuccess = false,
                    Message = "Invalid credentials"
                });
            }

            return Ok(new Response<TokenDto>
            {
                Value = result,
                IsSuccess = true
            });
        }

        // =========================
        // VALIDATE TOKEN
        // =========================
        [HttpGet("validate-token")]
        public IActionResult ValidateToken([FromQuery] string token)
        {
            var isValid = _service.ValidateToken(token);

            return Ok(new Response<bool>
            {
                Value = isValid,
                IsSuccess = true
            });
        }
    }
}