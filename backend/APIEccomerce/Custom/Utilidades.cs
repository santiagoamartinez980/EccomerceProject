using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using APIEccomerce.Models;
using Microsoft.IdentityModel.Tokens;

namespace APIEccomerce.Custom
{
    public class Utilidades
    {
        private readonly IConfiguration _config;

        public Utilidades(IConfiguration config)
        {
            _config = config;
        }

        public string HashearClave(string clave)
        {
            return BCrypt.Net.BCrypt.HashPassword(clave);
        }

        public bool VerificarClave(string clave, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(clave, hash);
        }

        public string GenerarJwt(Usuario usuario)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier,
                          usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Email, usuario.Correo),
                new Claim(ClaimTypes.Role, usuario.Rol.ToString())
            };

   
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

            
            var creds = new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256);

            
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public bool validarToken(string token)
        {
            var claimsPrincipal = new ClaimsPrincipal();
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.Zero,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_config["Jwt:Key"]!))
            };

            try
            {
                claimsPrincipal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}