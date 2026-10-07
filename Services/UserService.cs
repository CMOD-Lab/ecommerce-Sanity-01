using EcommerceWebApi.Entities;
using EcommerceWebApi.Utilities;
using System.Reflection;

namespace EcommerceWebApi.Services
{
    /// <summary>
    /// Handles user business logic.
    /// Application distribution and update checking is handled via AWS S3 + CloudFront
    /// through <see cref="IApplicationUpdateService"/>, replacing the former ClickOnce
    /// deployment model.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        // Replaces ClickOnce ApplicationDeployment.CurrentDeployment update checks:
        // version checking and package distribution are now handled by S3 + CloudFront.
        private readonly IApplicationUpdateService _updateService;

        public UserService(UnitOfWork unitOfWork, IApplicationUpdateService updateService)
        {
            _unitOfWork = unitOfWork;
            _updateService = updateService;
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
