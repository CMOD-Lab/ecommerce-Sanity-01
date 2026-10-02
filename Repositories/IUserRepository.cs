using EcommerceWebApi.Entities;
using JsonFlatFileDataStore;

namespace EcommerceWebApi.Repositories
{
    public interface IUserRepository
    {
        Task<bool> DeleteAsync(string id);
        IDocumentCollection<User> GetAll();
        User? GetById(string id);
        User? GetByName(string name);
        User? GetByToken(string token);
        // cr-dotnet-1000: Async variants added to support non-blocking data retrieval
        // in IAsyncAuthorizationFilter, ensuring efficient thread pool usage under
        // high load in AWS cloud auto-scaling scenarios.
        Task<User?> GetByIdAsync(string id);
        Task<User?> GetByTokenAsync(string token);
        Task<bool> InsertAsync(User user);
        Task<bool> UpdateAsync(User user);
    }
}
