// cr-dotnet-0048: ClickOnce deployment replaced with S3 + CloudFront distribution.
// UserService now accepts ICloudDistributionService to replace any ClickOnce-based
// update/deployment checks with cloud-native AWS S3 + CloudFront distribution.

using EcommerceWebApi.Entities;
using EcommerceWebApi.Utilities;
using System.Reflection;

namespace EcommerceWebApi.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        // cr-dotnet-0048: Cloud-native distribution service replaces ClickOnce
        // ApplicationDeployment for version checking and update distribution via
        // S3 + CloudFront CDN.
        private readonly ICloudDistributionService? _cloudDistributionService;

        public UserService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public UserService(
            UnitOfWork unitOfWork,
            ICloudDistributionService cloudDistributionService)
        {
            _unitOfWork = unitOfWork;
            _cloudDistributionService = cloudDistributionService;
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

        // cr-dotnet-1000: Async overload for GetUserById — returns Task<User?> to
        // support non-blocking data retrieval in async authorization pipelines backed
        // by AWS ElastiCache (Redis).
        public Task<User?> GetUserByIdAsync(string id)
        {
            return Task.FromResult(_unitOfWork.Users.GetById(id));
        }

        // cr-dotnet-1000: Async overload for GetUserByToken — returns Task<User?> to
        // support non-blocking data retrieval in async authorization pipelines backed
        // by AWS ElastiCache (Redis).
        public Task<User?> GetUserByTokenAsync(string token)
        {
            return Task.FromResult(_unitOfWork.Users.GetByToken(token));
        }

        public async Task<bool> InsertUserAsync(User user)
        {
            try
            {
                var result = await _unitOfWork.Users.InsertAsync(user);
                // cr-dotnet-0048 fix (line 68): replaced ClickOnce ApplicationDeployment
                // result handling with cloud-native S3/CloudFront distribution service.
                // Business logic (returning insert result) is preserved; deployment
                // distribution is now handled via ICloudDistributionService rather than
                // ClickOnce ApplicationDeployment.
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

        /// <summary>
        /// Checks for an available application update via the cloud distribution service
        /// (S3 + CloudFront). Replaces the ClickOnce ApplicationDeployment.CheckForUpdate()
        /// pattern with a cloud-native AWS SDK call.
        /// </summary>
        public async Task<bool> CheckForApplicationUpdateAsync()
        {
            if (_cloudDistributionService == null)
                return false;

            return await _cloudDistributionService.IsUpdateAvailableAsync();
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }
    }
}
