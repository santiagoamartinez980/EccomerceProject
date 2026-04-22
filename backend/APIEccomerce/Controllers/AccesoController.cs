using APIEccomerce.Custom;
using APIEccomerce.Data;
using APIEccomerce.Models;
using APIEccomerce.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIEccomerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous] 
    public class AccesoController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly Utilidades _utilidades;

        public AccesoController(AppDbContext context, Utilidades utilidades)
        {
            _context = context;
            _utilidades = utilidades;
        }

        [HttpPost("registrar")]
        public async Task<IActionResult> Registrar([FromBody] UsuarioDto dto)
        {
            
            var existe = await _context.Usuarios
                .AnyAsync(u => u.Correo == dto.Correo);

            if (existe)
                return BadRequest(new { mensaje = "El correo ya está registrado" });

            // Crear el usuario con la clave hasheada
            var usuario = new Usuario
            {
                Nombre = dto.Nombre,
                Apellidos = dto.Apellidos,
                Correo = dto.Correo,
                Clave = _utilidades.HashearClave(dto.Clave),
                Rol = Rol.Usuario 
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Usuario registrado correctamente" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            // Buscar usuario por correo
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Correo == dto.Correo);

            // Si no existe o la clave no coincide
            if (usuario == null || !_utilidades.VerificarClave(dto.Clave, usuario.Clave))
                return Unauthorized(new { mensaje = "Credenciales incorrectas" });

            // Generar y devolver el token
            var token = _utilidades.GenerarJwt(usuario);
            return Ok(new TokenDto { Token = token });
        }

        [HttpGet]
        [Route("ValidarToken")]
        public IActionResult ValidarToken([FromQuery] string token)
        {
            bool respuesta = _utilidades.validarToken(token);
            return StatusCode(StatusCodes.Status200OK, new { isSuccess = respuesta });
        }
    }
}