using EcommerceWebApi.Entities;
using EcommerceWebApi.Utilities;

namespace EcommerceWebApi.Services
{
    public interface IUserService
    {
        Task<bool> DeleteUserAsync(string id);
        void Dispose();
        List<User> GetAllUsers();
        List<User> GetPaginationUsers(PaginationFilter paginationFilter);
        User? GetUserById(string id);
        User? GetUserByName(string name);
        User? GetUserByToken(string token);
        // cr-dotnet-1000: Async variants added to support non-blocking data retrieval
        // in IAsyncAuthorizationFilter, ensuring efficient thread pool usage under
        // high load in AWS cloud auto-scaling scenarios.
        Task<User?> GetUserByIdAsync(string id);
        Task<User?> GetUserByTokenAsync(string token);
        Task<bool> InsertUserAsync(User user);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> UpdateUserPropertyAsync<T>(User user, string property, T value);
        Task<bool> UpdateUserTokenAsync(User user, RefreshToken? refreshToken);
    }
}
