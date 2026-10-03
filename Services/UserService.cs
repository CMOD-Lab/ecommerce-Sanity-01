// Migrated from ClickOnce deployment to AWS S3 + CloudFront distribution (cr-dotnet-0048).
// ClickOnce UpdateAsync pattern replaced with cloud-native AWS update service.
using EcommerceWebApi.Entities;
using EcommerceWebApi.Utilities;
using System.Reflection;

namespace EcommerceWebApi.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        // AWS S3 + CloudFront update service injected to replace ClickOnce deployment checks.
        // Version checking and update distribution are handled via S3-hosted packages
        // served through CloudFront CDN instead of ClickOnce UpdateAsync().
        private readonly IAwsUpdateService? _awsUpdateService;

        public UserService(UnitOfWork unitOfWork, IAwsUpdateService? awsUpdateService = null)
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

        /// <summary>
        /// Asynchronously retrieves a user by ID.
        /// cr-dotnet-1000: async version to support non-blocking calls in IAsyncAuthorizationFilter.
        /// </summary>
        public Task<User?> GetUserByIdAsync(string id)
        {
            return _unitOfWork.Users.GetByIdAsync(id);
        }

        public User? GetUserByToken(string token)
        {
            return _unitOfWork.Users.GetByToken(token);
        }

        /// <summary>
        /// Asynchronously retrieves a user by refresh token.
        /// cr-dotnet-1000: async version to support non-blocking calls in IAsyncAuthorizationFilter.
        /// </summary>
        public Task<User?> GetUserByTokenAsync(string token)
        {
            return _unitOfWork.Users.GetByTokenAsync(token);
        }

        public User? GetUserByName(string name)
        {
            return _unitOfWork.Users.GetByName(name);
        }

        public async Task<bool> InsertUserAsync(User user)
        {
            try
            {
                var result = await _unitOfWork.Users.InsertAsync(user);
                return result;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Updates a user record. Uses AWS S3 + CloudFront update service for cloud-native
        /// deployment distribution, replacing ClickOnce UpdateAsync() pattern (cr-dotnet-0048).
        /// </summary>
        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                // Check for available application updates via AWS S3 + CloudFront
                // replacing ClickOnce UpdateAsync() deployment pattern.
                if (_awsUpdateService != null)
                {
                    var updateAvailable = await _awsUpdateService.CheckForUpdateAsync();
                    if (updateAvailable)
                    {
                        await _awsUpdateService.ApplyUpdateAsync();
                    }
                }

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
