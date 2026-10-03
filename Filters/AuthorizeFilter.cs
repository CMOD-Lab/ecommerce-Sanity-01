using EcommerceWebApi.Authentication;
using EcommerceWebApi.Entities;
using EcommerceWebApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.IdentityModel.Tokens.Jwt;

namespace EcommerceWebApi.Filters
{
    /// <summary>
    /// cr-dotnet-1000: Converted from IAuthorizationFilter (synchronous) to
    /// IAsyncAuthorizationFilter (async/await) to avoid blocking thread pool threads
    /// during authorization checks. All data-access calls now use async methods,
    /// ensuring non-blocking operations and efficient thread pool usage under high
    /// load in AWS auto-scaling scenarios.
    /// </summary>
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

        // cr-dotnet-1000: Replaced synchronous OnAuthorization with async OnAuthorizationAsync
        // to eliminate blocking calls and support cloud-native async/await patterns.
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
                    // cr-dotnet-1000: replaced synchronous CheckRefreshToken with async version
                    var (refreshTokenValid, user) = await CheckRefreshTokenAsync(refreshTokenString, context);
                    if (!refreshTokenValid)
                    {
                        return;
                    }

                    if (string.IsNullOrEmpty(accessTokenString) && user != null)
                    {
                        _ = _jwtService.GenerateJWT(
                            user,
                            isSecondFactorChecked: true,
                            context.HttpContext
                        );

                        return;
                    }
                    else
                    {
                        // cr-dotnet-1000: replaced synchronous CheckAccessToken with async version
                        var (accessTokenValid, accessToken, currentUser) = await CheckAccessTokenAsync(
                            accessTokenString,
                            user,
                            context
                        );
                        if (!accessTokenValid)
                        {
                            return;
                        }

                        if (requiredRoles.Any() && !CheckRole(accessToken!, requiredRoles, context))
                        {
                            return;
                        }

                        _authService.CurrentUser = currentUser!;
                    }
                }
                else
                {
                    // cr-dotnet-1000: replaced synchronous CheckAccessToken with async version
                    var (accessTokenValid, accessToken, currentUser) = await CheckAccessTokenAsync(
                        accessTokenString,
                        null,
                        context
                    );
                    if (!accessTokenValid)
                    {
                        return;
                    }

                    if (requiredRoles.Any() && !CheckRole(accessToken!, requiredRoles, context))
                    {
                        return;
                    }

                    _authService.CurrentUser = currentUser!;

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

        /// <summary>
        /// cr-dotnet-1000: Async version of CheckRefreshToken. Replaces synchronous
        /// _userService.GetUserByToken() (line 118) with await _userService.GetUserByTokenAsync()
        /// to avoid blocking the thread pool during authorization.
        /// </summary>
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

                // cr-dotnet-1000: line 118 - replaced synchronous GetUserByToken with async GetUserByTokenAsync
                var user = await _userService.GetUserByTokenAsync(token: refreshTokenString).ConfigureAwait(false);

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

        /// <summary>
        /// cr-dotnet-1000: Async version of CheckAccessToken. Replaces synchronous
        /// _userService.GetUserById() (lines 129, 150, 162, 175, 219, 230, 257, 271)
        /// with await _userService.GetUserByIdAsync() to avoid blocking the thread pool.
        /// </summary>
        private async Task<(bool success, JwtSecurityToken? accessToken, User? currentUser)> CheckAccessTokenAsync(
            string? accessTokenString,
            User? user,
            AuthorizationFilterContext context
        )
        {
            try
            {
                var accessToken = new JwtSecurityTokenHandler().ReadJwtToken(accessTokenString);
                var accessTokenId = accessToken.Claims.FirstOrDefault(x => x.Type == "id");
                var isSecondFactorChecked = accessToken.Claims.FirstOrDefault(
                    x => x.Type == "status"
                );

                if (accessTokenId == null || isSecondFactorChecked == null)
                {
                    return (false, null, null);
                }

                // cr-dotnet-1000: lines 129, 150, 162, 175, 219, 230, 257, 271 -
                // replaced synchronous GetUserById with async GetUserByIdAsync
                var currentUser = await _userService.GetUserByIdAsync(accessTokenId.Value).ConfigureAwait(false);

                if (
                    (
                        user != null
                        && accessTokenId.Value == user.Id
                        && isSecondFactorChecked.Value == "True"
                    ) || (user == null && currentUser != null)
                )
                {
                    return (true, accessToken, currentUser);
                }

                context.Result = new ContentResult()
                {
                    Content = "Unauthorized",
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return (false, null, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{ex.Message} - {ex.InnerException?.Message}");
                context.Result = new ContentResult()
                {
                    Content = "Unauthorized",
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return (false, null, null);
            }
        }

        private bool CheckRole(
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

                return true;
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
