using EcommerceWebApi.Data;
using EcommerceWebApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcommerceWebApi.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public IQueryable<User> GetAll()
        {
            try
            {
                return _context.Users.Include(u => u.RefreshToken).AsQueryable();
            }
            catch
            {
                throw;
            }
        }

        public User? GetById(string id)
        {
            try
            {
                return _context.Users
                    .Include(u => u.RefreshToken)
                    .FirstOrDefault(x => x.Id == id);
            }
            catch
            {
                throw;
            }
        }

        public User? GetByToken(string token)
        {
            try
            {
                return _context.Users
                    .Include(u => u.RefreshToken)
                    .FirstOrDefault(x => x.RefreshToken != null && x.RefreshToken.Token == token);
            }
            catch
            {
                throw;
            }
        }

        public User? GetByName(string name)
        {
            try
            {
                return _context.Users
                    .Include(u => u.RefreshToken)
                    .FirstOrDefault(x => x.Username == name);
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> InsertAsync(User user)
        {
            try
            {
                await _context.Users.AddAsync(user);
                return true;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> UpdateAsync(User user)
        {
            try
            {
                _context.Users.Update(user);
                return await Task.FromResult(true);
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null) return false;
                _context.Users.Remove(user);
                return true;
            }
            catch
            {
                throw;
            }
        }
    }
}
