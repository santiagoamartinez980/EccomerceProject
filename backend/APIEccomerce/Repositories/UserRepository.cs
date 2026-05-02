using APIEccomerce.Data;
using APIEccomerce.Models;
using APIEccomerce.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace APIEccomerce.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        // check if email already exists
        public async Task<bool> ExistsByEmail(string email)
        {
            return await _context.Users
                .AnyAsync(u => u.Email == email);
        }

        // get user by email
        public async Task<User?> GetByEmail(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        // create new user
        public async Task<User> Create(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }
    }
}