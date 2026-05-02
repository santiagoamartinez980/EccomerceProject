using APIEccomerce.Models.DTOs;

namespace APIEccomerce.Services.Interfaces
{
    public interface IAccessService
    {
        Task<string> Register(UserDto dto);

        Task<TokenDto?> Login(LoginDto dto);

        bool ValidateToken(string token);
    }
}
