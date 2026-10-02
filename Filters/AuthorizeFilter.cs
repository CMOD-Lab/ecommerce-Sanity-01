// cr-dotnet-1000: Converted AuthorizeFilter from IAuthorizationFilter (synchronous) to
// IAsyncAuthorizationFilter (async) to eliminate blocking calls during request processing.
// All synchronous _userService.GetUserByToken() and _userService.GetUserById() calls have
// been replaced with their async counterparts (GetUserByTokenAsync / GetUserByIdAsync),
// backed by non-blocking data retrieval to ensure efficient thread pool usage under high
// load in AWS cloud auto-scaling scenarios (ECS / Fargate / ElastiCache).
using EcommerceWebApi.Authentication;
using EcommerceWebApi.Entities;
using EcommerceWebApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.IdentityModel.Tokens.Jwt;

namespace EcommerceWebApi.Filters
{
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

        // cr-dotnet-1000: Replaced synchronous OnAuthorization(AuthorizationFilterContext) with
        // async OnAuthorizationAsync(AuthorizationFilterContext) to avoid blocking the thread pool.
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
                    // cr-dotnet-1000: Line 118 — replaced synchronous CheckRefreshToken with
                    // async CheckRefreshTokenAsync to avoid blocking the thread pool.
                    if (!await CheckRefreshTokenAsync(refreshTokenString, context, out User? user))
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
                        // cr-dotnet-1000: Line 129 — replaced synchronous CheckAccessToken with
                        // async CheckAccessTokenAsync to avoid blocking the thread pool.
                        if (
                            !await CheckAccessTokenAsync(
                                accessTokenString,
                                user,
                                context,
                                out JwtSecurityToken accessToken,
                                out User currentUser
                            )
                        )
                        {
                            return;
                        }

                        if (requiredRoles.Any() && !CheckRole(accessToken, requiredRoles, context))
                        {
                            return;
                        }

                        _authService.CurrentUser = currentUser;
                    }
                }
                else
                {
                    // cr-dotnet-1000: Line 150 — replaced synchronous CheckAccessToken with
                    // async CheckAccessTokenAsync to avoid blocking the thread pool.
                    if (
                        !await CheckAccessTokenAsync(
                            accessTokenString,
                            null,
                            context,
                            out JwtSecurityToken accessToken,
                            out User currentUser
                        )
                    )
                    {
                        return;
                    }

                    if (requiredRoles.Any() && !CheckRole(accessToken, requiredRoles, context))
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

        // cr-dotnet-1000: Lines 162, 175 — Converted CheckRefreshToken to async
        // CheckRefreshTokenAsync. Replaced synchronous _userService.GetUserByToken()
        // with await _userService.GetUserByTokenAsync() for non-blocking data retrieval.
        private async Task<bool> CheckRefreshTokenAsync(
            string? refreshTokenString,
            AuthorizationFilterContext context,
            out User? user
        )
        {
            user = null!;
            try
            {
                user = null;
                if (string.IsNullOrEmpty(refreshTokenString))
                {
                    context.Result = new ContentResult()
                    {
                        Content = "Unauthorized",
                        StatusCode = StatusCodes.Status401Unauthorized
                    };
                    return false;
                }

                // cr-dotnet-1000: Line 175 — replaced synchronous GetUserByToken with
                // async GetUserByTokenAsync to avoid blocking the thread pool.
                user = await _userService.GetUserByTokenAsync(token: refreshTokenString).ConfigureAwait(false);

                if (user == null || user.RefreshToken.Expires < DateTimeOffset.UtcNow.UtcDateTime)
                {
                    context.Result = new ContentResult()
                    {
                        Content = "Unauthorized",
                        StatusCode = StatusCodes.Status401Unauthorized
                    };
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{ex.Message} - {ex.InnerException?.Message}");
                context.Result = new ContentResult()
                {
                    Content = "Unauthorized",
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return false;
            }
        }

        // cr-dotnet-1000: Lines 219, 230, 257, 271 — Converted CheckAccessToken to async
        // CheckAccessTokenAsync. Replaced synchronous _userService.GetUserById()
        // with await _userService.GetUserByIdAsync() for non-blocking data retrieval.
        private async Task<bool> CheckAccessTokenAsync(
            string? accessTokenString,
            User? user,
            AuthorizationFilterContext context,
            out JwtSecurityToken accessToken,
            out User currentUser
        )
        {
            accessToken = null!;
            currentUser = null!;
            try
            {
                accessToken = new JwtSecurityTokenHandler().ReadJwtToken(accessTokenString);
                var accessTokenId = accessToken.Claims.FirstOrDefault(x => x.Type == "id");
                var isSecondFactorChecked = accessToken.Claims.FirstOrDefault(
                    x => x.Type == "status"
                );

                if (accessTokenId == null || isSecondFactorChecked == null)
                {
                    return false;
                }

                // cr-dotnet-1000: Line 230 — replaced synchronous GetUserById with
                // async GetUserByIdAsync to avoid blocking the thread pool.
                currentUser = (await _userService.GetUserByIdAsync(accessTokenId.Value).ConfigureAwait(false))!;

                if (
                    (
                        user != null
                        && accessTokenId.Value == user.Id
                        && isSecondFactorChecked.Value == "True"
                    ) || (user == null && currentUser != null)
                )
                {
                    return true;
                }
                context.Result = new ContentResult()
                {
                    Content = "Unauthorized",
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{ex.Message} - {ex.InnerException?.Message}");
                context.Result = new ContentResult()
                {
                    Content = "Unauthorized",
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return false;
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
