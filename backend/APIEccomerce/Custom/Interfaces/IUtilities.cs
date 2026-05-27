using APIEccomerce.Models;

namespace APIEccomerce.Custom
{
    public interface IUtilities
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
        string GenerateJwt(User user);
        bool ValidateToken(string token);
    }
}