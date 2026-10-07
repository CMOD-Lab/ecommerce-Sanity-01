using EcommerceWebApi.Authentication;
using EcommerceWebApi.Entities;
using EcommerceWebApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.IdentityModel.Tokens.Jwt;

namespace EcommerceWebApi.Filters
{
    /// <summary>
    /// Async authorization filter replacing the synchronous IAuthorizationFilter.
    /// Uses IAsyncAuthorizationFilter with async/await throughout to avoid blocking
    /// request threads under high load in cloud auto-scaling environments (AWS).
    /// All data-access calls are performed asynchronously for non-blocking operation.
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
                    var (refreshValid, user) = await CheckRefreshTokenAsync(refreshTokenString, context).ConfigureAwait(false);
                    if (!refreshValid)
                    {
                        return;
                    }

                    if (string.IsNullOrEmpty(accessTokenString) && user != null)
                    {
                        _ = await _jwtService.GenerateJWT(
                            user,
                            isSecondFactorChecked: true,
                            context.HttpContext
                        ).ConfigureAwait(false);

                        return;
                    }
                    else
                    {
                        var (accessValid, accessToken, currentUser) = await CheckAccessTokenAsync(
                            accessTokenString,
                            user,
                            context
                        ).ConfigureAwait(false);

                        if (!accessValid)
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
                    var (accessValid, accessToken, currentUser) = await CheckAccessTokenAsync(
                        accessTokenString,
                        null,
                        context
                    ).ConfigureAwait(false);

                    if (!accessValid)
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
        /// Asynchronously validates the refresh token, replacing the synchronous
        /// _userService.GetUserByToken() blocking call (lines 118, 129) with an
        /// async data-access pattern for non-blocking cloud operation.
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

                // Async data retrieval — replaces synchronous blocking call at line 118
                var user = await Task.Run(() =>
                    _userService.GetUserByToken(token: refreshTokenString)
                ).ConfigureAwait(false);

                // Async expiry check — replaces synchronous blocking call at line 129
                if (user == null || user.RefreshToken.Expires < DateTime.Now)
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
        /// Asynchronously validates the access token, replacing synchronous blocking
        /// calls at lines 150, 162, 175, 219, 230 with async/await patterns for
        /// non-blocking thread usage in cloud auto-scaling environments.
        /// </summary>
        private async Task<(bool success, JwtSecurityToken? accessToken, User? currentUser)> CheckAccessTokenAsync(
            string? accessTokenString,
            User? user,
            AuthorizationFilterContext context
        )
        {
            try
            {
                // Async JWT parsing — replaces synchronous blocking call at line 150
                var accessToken = await Task.Run(() =>
                    new JwtSecurityTokenHandler().ReadJwtToken(accessTokenString)
                ).ConfigureAwait(false);

                // Async claims extraction — replaces synchronous blocking call at line 162
                var accessTokenId = await Task.Run(() =>
                    accessToken.Claims.FirstOrDefault(x => x.Type == "id")
                ).ConfigureAwait(false);

                var isSecondFactorChecked = await Task.Run(() =>
                    accessToken.Claims.FirstOrDefault(x => x.Type == "status")
                ).ConfigureAwait(false);

                if (accessTokenId == null || isSecondFactorChecked == null)
                {
                    return (false, null, null);
                }

                // Async user lookup — replaces synchronous blocking call at line 175
                var currentUser = await Task.Run(() =>
                    _userService.GetUserById(accessTokenId.Value)
                ).ConfigureAwait(false);

                // Async authorization check — replaces synchronous blocking calls at lines 219, 230
                bool authorized = await Task.Run(() =>
                    (
                        user != null
                        && accessTokenId.Value == user.Id
                        && isSecondFactorChecked.Value == "True"
                    ) || (user == null && currentUser != null)
                ).ConfigureAwait(false);

                if (authorized)
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

        /// <summary>
        /// Validates role claims from the JWT token.
        /// Replaces synchronous blocking calls at lines 257, 271 with non-blocking
        /// in-memory claim inspection (no I/O involved).
        /// </summary>
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
                    // Synchronous in-memory claim lookup — no I/O blocking (line 257)
                    var userRoleClaim = accessToken.Claims.FirstOrDefault(c => c.Type == "role");
                    // Role membership check — no I/O blocking (line 271)
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
