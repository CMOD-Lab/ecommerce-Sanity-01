using EcommerceWebApi.Entities;
using JsonFlatFileDataStore;

namespace EcommerceWebApi.Repositories
{
    public interface IUserRepository
    {
        Task<bool> DeleteAsync(string id);
        IDocumentCollection<User> GetAll();
        User? GetById(string id);
        Task<User?> GetByIdAsync(string id);
        User? GetByName(string name);
        User? GetByToken(string token);
        Task<User?> GetByTokenAsync(string token);
        Task<bool> InsertAsync(User user);
        Task<bool> UpdateAsync(User user);
    }
}
