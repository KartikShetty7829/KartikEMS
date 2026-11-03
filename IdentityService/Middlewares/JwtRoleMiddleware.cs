using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IdentityService.Middlewares
{
    /// <summary>
    /// Custom Middleware: Validates JWT token and checks role access for IdentityController endpoints.
    /// - Public endpoints: login, adminlogin, forgot-password
    /// - Protected endpoint: register (Admin only)
    /// </summary>
    public class JwtRoleMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _config;
        private readonly ILogger<JwtRoleMiddleware> _logger;

        // ✅ Public routes (no token required)
        private static readonly HashSet<string> _publicRoutes = new(StringComparer.OrdinalIgnoreCase)
        {
            "/api/identity/login",
            "/api/identity/adminlogin",
            "/api/identity/forgot-password"
        };

        // ✅ Protected routes → Required Role mapping
        private static readonly Dictionary<string, string> _routeRoles = new(StringComparer.OrdinalIgnoreCase)
        {
            { "/api/identity/register", "Admin" }
        };

        public JwtRoleMiddleware(RequestDelegate next, IConfiguration config, ILogger<JwtRoleMiddleware> logger)
        {
            _next = next;
            _config = config;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value ?? string.Empty;

            // ✅ Skip Swagger & health checks
            if (path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/health", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            // ✅ Allow public routes
            if (_publicRoutes.Contains(path))
            {
                await _next(context);
                return;
            }

            // ✅ Check if route requires a role
            var cleanPath = path.Split('?')[0].ToLower();
            _routeRoles.TryGetValue(cleanPath, out var requiredRole);

            if (requiredRole == null)
            {
                await _next(context);
                return;
            }

            // ─── From here: route requires role ───────────────────

            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { Success = false, Message = "Unauthorized: No token provided." });
                return;
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();
            var principal = ValidateToken(token);

            if (principal == null)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { Success = false, Message = "Unauthorized: Token invalid or expired." });
                return;
            }

            var userRole = principal.FindFirst(ClaimTypes.Role)?.Value;
            var userName = principal.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(userRole) || !userRole.Equals(requiredRole, StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new { Success = false, Message = $"Forbidden: Required role '{requiredRole}'." });
                return;
            }

            context.User = principal;
            _logger.LogInformation("✅ User '{Username}' (Role: {Role}) accessed '{Path}'", userName, userRole, path);

            await _next(context);
        }

        private ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

                var validationParams = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _config["Jwt:Issuer"],
                    ValidAudience = _config["Jwt:Audience"],
                    IssuerSigningKey = key,
                    ClockSkew = TimeSpan.Zero
                };

                var handler = new JwtSecurityTokenHandler();
                return handler.ValidateToken(token, validationParams, out _);
            }
            catch
            {
                return null;
            }
        }
    }

    public static class JwtRoleMiddlewareExtensions
    {
        public static IApplicationBuilder UseJwtRoleMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<JwtRoleMiddleware>();
        }
    }
}
