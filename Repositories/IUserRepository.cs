using EcommerceWebApi.Entities;

namespace EcommerceWebApi.Repositories
{
    public interface IUserRepository
    {
        Task<bool> DeleteAsync(string id);
        IQueryable<User> GetAll();
        User? GetById(string id);
        User? GetByName(string name);
        User? GetByToken(string token);
        Task<bool> InsertAsync(User user);
        Task<bool> UpdateAsync(User user);
    }
}
