using Microsoft.AspNetCore.Mvc;
using MiRoti.Models;
using MiRoti.Services;

namespace MiRoti.ControllersApi
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthApiController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthApiController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Usuario login)
        {
            var token = await _authService.AutenticarAsync(login.Email, login.Contrasenia);
            if (token == null)
                return Unauthorized(new { mensaje = "Credenciales inválidas" });

            return Ok(new { token });
        }
    }
}
