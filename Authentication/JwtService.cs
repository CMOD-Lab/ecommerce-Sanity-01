using EcommerceWebApi.Entities;
using EcommerceWebApi.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace EcommerceWebApi.Authentication
{
    public class JwtService : IJwtService
    {
        private readonly AppSettings _applicationSettings;
        private readonly UserService _userService;
        // Redis-backed distributed cache replaces in-memory refresh token state,
        // enabling token survival across Kubernetes pod restarts and replica scaling on EKS.
        private readonly IDistributedCache _distributedCache;

        private const string RefreshTokenCacheKeyPrefix = "circuit:refreshToken:";

        public JwtService(
            IOptions<AppSettings> applicationSettings,
            UserService userService,
            IDistributedCache distributedCache)
        {
            _applicationSettings = applicationSettings.Value;
            _userService = userService;
            _distributedCache = distributedCache;
        }

        public async Task<bool> GenerateJWT(
            User user,
            bool isSecondFactorChecked,
            HttpContext context
        )
        {
            // Create JWT
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_applicationSettings.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                    new[]
                    {
                        new Claim("id", user.Id.ToString()),
                        new Claim("status", isSecondFactorChecked.ToString()),
                        new Claim("role", user.Role)
                    }
                ),
                Expires = DateTime.Now.AddMinutes(15),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha512Signature
                )
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var encrypterToken = tokenHandler.WriteToken(token);

            // Set JWT
            SetJWT(encrypterToken, context);

            // Create & set refresh token
            if (isSecondFactorChecked)
            {
                var refreshToken = GenerateRefreshToken();
                return await SetRefreshToken(refreshToken, user, context);
            }

            return true;
        }

        public void SetJWT(string encrypterToken, HttpContext context)
        {
            // Set JWT to cookie
            context.Response.Cookies.Append(
                "access_token",
                encrypterToken,
                new CookieOptions
                {
                    Expires = DateTime.Now.AddMinutes(15),
                    HttpOnly = true,
                    Secure = true,
                    IsEssential = true,
                    SameSite = SameSiteMode.None
                }
            );
        }

        // Previously: private static RefreshToken GenerateRefreshToken()
        // Refresh token generation is now paired with Redis-backed persistence via SetRefreshToken,
        // replacing in-memory storage to support circuit survival across pod restarts on EKS.
        private static RefreshToken GenerateRefreshToken()
        {
            // Create refresh token
            var refreshToken = new RefreshToken()
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.Now.AddDays(7),
                Created = DateTime.Now
            };

            return refreshToken;
        }

        public async Task<bool> SetRefreshToken(
            RefreshToken refreshToken,
            User user,
            HttpContext context
        )
        {
            // Set refresh token to cookie
            context.Response.Cookies.Append(
                "refresh_token",
                refreshToken.Token,
                new CookieOptions
                {
                    Expires = refreshToken.Expires,
                    HttpOnly = true,
                    Secure = true,
                    IsEssential = true,
                    SameSite = SameSiteMode.None
                }
            );

            // Persist refresh token to Redis distributed cache so it survives pod restarts
            // and is accessible across all replicas on EKS with Amazon ElastiCache for Redis.
            var cacheKey = $"{RefreshTokenCacheKeyPrefix}{user.Id}";
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = refreshToken.Expires
            };
            await _distributedCache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(refreshToken),
                cacheOptions
            );

            // Set refresh token to user (database persistence)
            return await _userService.UpdateUserTokenAsync(user, refreshToken);
        }

        public async Task<bool> RevokeToken(User user, HttpContext context)
        {
            // Revoke all tokens from cookies, Redis cache, and user record
            var result = await _userService.UpdateUserTokenAsync(user, null);
            if (!result)
            {
                return false;
            }

            // Remove refresh token from Redis distributed cache
            var cacheKey = $"{RefreshTokenCacheKeyPrefix}{user.Id}";
            await _distributedCache.RemoveAsync(cacheKey);

            context.Response.Cookies.Delete("access_token");
            context.Response.Cookies.Delete("refresh_token");
            return true;
        }

        public User? GetUserFromJWT(HttpContext context)
        {
            // Get user from JWT
            var tokenString = context.Request.Cookies["access_token"];
            if (string.IsNullOrEmpty(tokenString))
            {
                return null;
            }

            try
            {
                // Read token
                var token = new JwtSecurityTokenHandler().ReadJwtToken(tokenString);
                return _userService.GetUserById(token.Claims.First(x => x.Type == "id").Value);
            }
            catch
            {
                return null;
            }
        }

        public string? GetTotpStatusFromJWT(HttpContext context)
        {
            // Get auth status from JWT
            var tokenString = context.Request.Cookies["access_token"];
            if (string.IsNullOrEmpty(tokenString))
            {
                return null;
            }

            try
            {
                // Read token
                var token = new JwtSecurityTokenHandler().ReadJwtToken(tokenString);
                return token.Claims.First(x => x.Type == "status").Value;
            }
            catch
            {
                return null;
            }
        }
    }
}
