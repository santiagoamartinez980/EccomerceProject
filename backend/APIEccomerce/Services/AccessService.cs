using APIEccomerce.Models;
using APIEccomerce.Models.DTOs;
using APIEccomerce.Repositories.Interfaces;
using APIEccomerce.Services.Interfaces;
using APIEccomerce.Custom;

namespace APIEccomerce.Services
{
    public class AccessService : IAccessService
    {
        private readonly IUserRepository _repo;
        private readonly Utilities _utils;

        public AccessService(IUserRepository repo, Utilities utils)
        {
            _repo = repo;
            _utils = utils;

        }

        // register new user
        public async Task<string> Register(UserDto dto)
        {
            var exists = await _repo.ExistsByEmail(dto.Email);

            if (exists)
                throw new Exception("Email already exists");

            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Password = _utils.HashPassword(dto.Password),
                Role = Role.User
            };

            await _repo.Create(user);

            return "User registered successfully";
        }

        // login user
        public async Task<TokenDto?> Login(LoginDto dto)
        {
            var user = await _repo.GetByEmail(dto.Email);

            if (user == null || !_utils.VerifyPassword(dto.Password, user.Password))
                return null;

            var token = _utils.GenerateJwt(user);

            return new TokenDto { Token = token };
        }

        // validate jwt
        public bool ValidateToken(string token)
        {
            return _utils.ValidateToken(token);
        }
    }
}