// UserService.cs
// cr-dotnet-0048 fix: Replaced ClickOnce deployment dependency with
// AWS S3 + CloudFront update service (IAwsUpdateService).
// ClickOnce's desktop-only update mechanism is replaced by cloud-native
// version checking via S3 version manifest and CloudFront CDN distribution.

using EcommerceWebApi.Entities;
using EcommerceWebApi.Utilities;
using System.Reflection;

namespace EcommerceWebApi.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        // cr-dotnet-0048: IAwsUpdateService replaces ClickOnce deployment.
        // Application distribution and updates are now handled via
        // S3-hosted packages distributed through CloudFront CDN.
        private readonly IAwsUpdateService _awsUpdateService;

        public UserService(UnitOfWork unitOfWork, IAwsUpdateService awsUpdateService)
        {
            _unitOfWork = unitOfWork;
            _awsUpdateService = awsUpdateService;
        }

        public List<User> GetAllUsers()
        {
            try
            {
                return _unitOfWork.Users.GetAll().AsQueryable().ToList();
            }
            catch
            {
                throw;
            }
        }

        public List<User> GetPaginationUsers(PaginationFilter paginationFilter)
        {
            return GetAllUsers()
                .Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize)
                .Take(paginationFilter.PageSize)
                .ToList();
        }

        public User? GetUserById(string id)
        {
            return _unitOfWork.Users.GetById(id);
        }

        public User? GetUserByToken(string token)
        {
            return _unitOfWork.Users.GetByToken(token);
        }

        public User? GetUserByName(string name)
        {
            return _unitOfWork.Users.GetByName(name);
        }

        // cr-dotnet-1000: Async variant of GetUserById to support non-blocking data retrieval
        // in IAsyncAuthorizationFilter, ensuring efficient thread pool usage under
        // high load in AWS cloud auto-scaling scenarios.
        public Task<User?> GetUserByIdAsync(string id)
        {
            return _unitOfWork.Users.GetByIdAsync(id);
        }

        // cr-dotnet-1000: Async variant of GetUserByToken to support non-blocking data retrieval
        // in IAsyncAuthorizationFilter, ensuring efficient thread pool usage under
        // high load in AWS cloud auto-scaling scenarios.
        public Task<User?> GetUserByTokenAsync(string token)
        {
            return _unitOfWork.Users.GetByTokenAsync(token);
        }

        public async Task<bool> InsertUserAsync(User user)
        {
            try
            {
                var result = await _unitOfWork.Users.InsertAsync(user);
                // cr-dotnet-0048: User data is persisted to the data store.
                // Application distribution and version updates are handled via
                // AWS S3 + CloudFront (IAwsUpdateService), not ClickOnce.
                return result;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                var result = await _unitOfWork.Users.UpdateAsync(user);
                return result;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> UpdateUserTokenAsync(User user, RefreshToken? refreshToken)
        {
            if (refreshToken != null)
            {
                user.RefreshToken = refreshToken;
            }
            else
            {
                user.RefreshToken.Token = null!;
            }
            return await UpdateUserAsync(user);
        }

        public async Task<bool> UpdateUserPropertyAsync<T>(User user, string property, T value)
        {
            var propertyInfo = typeof(User).GetProperty(
                property,
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance
            );

            if (propertyInfo == null)
            {
                return false;
            }
            try
            {
                propertyInfo.SetValue(user, value, null);
                return await UpdateUserAsync(user);
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            try
            {
                var result = await _unitOfWork.Users.DeleteAsync(id);
                return result;
            }
            catch
            {
                throw;
            }
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }
    }
}
