using EcommerceWebApi.Authentication;
using EcommerceWebApi.Entities;
using EcommerceWebApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.IdentityModel.Tokens.Jwt;

namespace EcommerceWebApi.Filters
{
    // cr-dotnet-1000: Converted from IAuthorizationFilter (synchronous) to
    // IAsyncAuthorizationFilter (async/await) to avoid blocking request threads
    // under cloud auto-scaling scenarios. All synchronous service calls replaced
    // with their async counterparts backed by AWS ElastiCache (Redis) data layer.
    public class AuthorizeFilter : IAsyncAuthorizationFilter
    {
        private readonly AuthService _authService;
        private readonly UserService _userService;
        private readonly JwtService _jwtService;
        private readonly ILogger<AuthorizeFilter> _logger;

        public AuthorizeFilter(
            AuthService authService,
            UserService userService,
            JwtService jwtService,
            ILogger<AuthorizeFilter> logger
        )
        {
            _authService = authService;
            _userService = userService;
            _jwtService = jwtService;
            _logger = logger;
        }

        // cr-dotnet-1000 (lines 118, 129, 150, 162, 175): Replaced synchronous
        // OnAuthorization with async OnAuthorizationAsync. All blocking service
        // calls are now awaited for non-blocking thread-pool usage.
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            try
            {
                var allowAnonymous = context.ActionDescriptor.EndpointMetadata
                    .OfType<AllowAnonymousAttribute>()
                    .Any();
                if (allowAnonymous)
                {
                    return;
                }

                var allowFirstFactor = context.ActionDescriptor.EndpointMetadata
                    .OfType<AllowFirstFactorAttribute>()
                    .Any();

                var requiredRoles = context.ActionDescriptor.EndpointMetadata
                    .OfType<AuthorizeRoleAttribute>()
                    .Select(a => a.Role);

                var accessTokenString = context.HttpContext.Request.Cookies["access_token"];
                var refreshTokenString = context.HttpContext.Request.Cookies["refresh_token"];

                if (!allowFirstFactor)
                {
                    // cr-dotnet-1000 (line 118): Awaited async CheckRefreshTokenAsync
                    // instead of synchronous CheckRefreshToken.
                    var (refreshOk, user) = await CheckRefreshTokenAsync(refreshTokenString, context);
                    if (!refreshOk)
                    {
                        return;
                    }

                    if (string.IsNullOrEmpty(accessTokenString) && user != null)
                    {
                        // cr-dotnet-1000 (line 129): Awaited async GenerateJWT call
                        // instead of fire-and-forget synchronous invocation.
                        _ = await _jwtService.GenerateJWTAsync(
                            user,
                            isSecondFactorChecked: true,
                            context.HttpContext
                        );

                        return;
                    }
                    else
                    {
                        // cr-dotnet-1000 (line 150): Awaited async CheckAccessTokenAsync
                        // instead of synchronous CheckAccessToken.
                        var (accessOk, accessToken, currentUser) = await CheckAccessTokenAsync(
                            accessTokenString,
                            user,
                            context
                        );
                        if (!accessOk)
                        {
                            return;
                        }

                        // cr-dotnet-1000 (line 162): Awaited async CheckRoleAsync
                        // instead of synchronous CheckRole.
                        if (requiredRoles.Any() && !await CheckRoleAsync(accessToken!, requiredRoles, context))
                        {
                            return;
                        }

                        _authService.CurrentUser = currentUser;
                    }
                }
                else
                {
                    // cr-dotnet-1000 (line 175): Awaited async CheckAccessTokenAsync
                    // instead of synchronous CheckAccessToken.
                    var (accessOk, accessToken, currentUser) = await CheckAccessTokenAsync(
                        accessTokenString,
                        null,
                        context
                    );
                    if (!accessOk)
                    {
                        return;
                    }

                    if (requiredRoles.Any() && !await CheckRoleAsync(accessToken!, requiredRoles, context))
                    {
                        return;
                    }

                    _authService.CurrentUser = currentUser;

                    context.Result = new ContentResult()
                    {
                        Content = "Need second factor authentication",
                        StatusCode = StatusCodes.Status200OK
                    };
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{ex.Message} - {ex.InnerException?.Message}");
                context.Result = new ContentResult()
                {
                    Content = "Unauthorized",
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return;
            }
        }

        // cr-dotnet-1000 (lines 219, 230): Converted CheckRefreshToken to async.
        // _userService.GetUserByToken is now awaited via GetUserByTokenAsync for
        // non-blocking data retrieval backed by AWS ElastiCache (Redis).
        private async Task<(bool success, User? user)> CheckRefreshTokenAsync(
            string? refreshTokenString,
            AuthorizationFilterContext context
        )
        {
            try
            {
                if (string.IsNullOrEmpty(refreshTokenString))
                {
                    context.Result = new ContentResult()
                    {
                        Content = "Unauthorized",
                        StatusCode = StatusCodes.Status401Unauthorized
                    };
                    return (false, null);
                }

                // cr-dotnet-1000 (line 219): Replaced synchronous GetUserByToken with
                // async GetUserByTokenAsync to avoid blocking the thread pool.
                var user = await _userService.GetUserByTokenAsync(token: refreshTokenString);

                // cr-dotnet-1000 (line 230): Replaced DateTime.Now with
                // DateTimeOffset.UtcNow.UtcDateTime for cloud-safe UTC comparison.
                if (user == null || user.RefreshToken.Expires < DateTimeOffset.UtcNow.UtcDateTime)
                {
                    context.Result = new ContentResult()
                    {
                        Content = "Unauthorized",
                        StatusCode = StatusCodes.Status401Unauthorized
                    };
                    return (false, null);
                }

                return (true, user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{ex.Message} - {ex.InnerException?.Message}");
                context.Result = new ContentResult()
                {
                    Content = "Unauthorized",
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return (false, null);
            }
        }

        // cr-dotnet-1000 (lines 257, 271): Converted CheckAccessToken to async.
        // _userService.GetUserById is now awaited via GetUserByIdAsync for
        // non-blocking data retrieval backed by AWS ElastiCache (Redis).
        private async Task<(bool success, JwtSecurityToken? accessToken, User currentUser)> CheckAccessTokenAsync(
            string? accessTokenString,
            User? user,
            AuthorizationFilterContext context
        )
        {
            JwtSecurityToken? accessToken = null;
            try
            {
                accessToken = new JwtSecurityTokenHandler().ReadJwtToken(accessTokenString);
                var accessTokenId = accessToken.Claims.FirstOrDefault(x => x.Type == "id");
                var isSecondFactorChecked = accessToken.Claims.FirstOrDefault(
                    x => x.Type == "status"
                );

                if (accessTokenId == null || isSecondFactorChecked == null)
                {
                    return (false, null, null!);
                }

                // cr-dotnet-1000 (line 257): Replaced synchronous GetUserById with
                // async GetUserByIdAsync to avoid blocking the thread pool.
                var currentUser = await _userService.GetUserByIdAsync(accessTokenId.Value);

                if (
                    (
                        user != null
                        && accessTokenId.Value == user.Id
                        && isSecondFactorChecked.Value == "True"
                    ) || (user == null && currentUser != null)
                )
                {
                    return (true, accessToken, currentUser!);
                }

                // cr-dotnet-1000 (line 271): Replaced synchronous result assignment
                // with async-compatible pattern inside async method.
                context.Result = new ContentResult()
                {
                    Content = "Unauthorized",
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return (false, null, null!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{ex.Message} - {ex.InnerException?.Message}");
                context.Result = new ContentResult()
                {
                    Content = "Unauthorized",
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return (false, null, null!);
            }
        }

        private async Task<bool> CheckRoleAsync(
            JwtSecurityToken accessToken,
            IEnumerable<string>? requiredRoles,
            AuthorizationFilterContext context
        )
        {
            try
            {
                if (requiredRoles == null)
                {
                    return false;
                }

                if (requiredRoles.Any())
                {
                    var userRoleClaim = accessToken.Claims.FirstOrDefault(c => c.Type == "role");
                    if (userRoleClaim == null || !requiredRoles.Contains(userRoleClaim.Value))
                    {
                        context.Result = new ContentResult()
                        {
                            Content = "Forbidden",
                            StatusCode = StatusCodes.Status403Forbidden
                        };
                        return false;
                    }
                }

                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{ex.Message} - {ex.InnerException?.Message}");
                context.Result = new ContentResult()
                {
                    Content = "Forbidden",
                    StatusCode = StatusCodes.Status403Forbidden
                };
                return false;
            }
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class AllowAnonymousAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class AllowFirstFactorAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class AuthorizeRoleAttribute : Attribute
    {
        public string Role { get; }

        public AuthorizeRoleAttribute(string role)
        {
            Role = role;
        }
    }
}
