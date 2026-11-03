using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EmployeeServiceDepartmentService.Middlewares
{
    /// <summary>
    /// Custom Middleware: Validates JWT token and checks role access for every API request.
    /// - If no token → 401 Unauthorized
    /// - If token is invalid/expired → 401 Unauthorized
    /// - If role doesn't match required role for route → 403 Forbidden
    /// - Public routes (no auth needed) are configured in _publicRoutes
    /// </summary>
    public class JwtRoleMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _config;
        private readonly ILogger<JwtRoleMiddleware> _logger;

        // ✅ Routes that are accessible WITHOUT a token (public)
        private static readonly HashSet<string> _publicRoutes = new(StringComparer.OrdinalIgnoreCase)
        {
            // Add any public routes here e.g. health checks
            // "/api/employee/some-public-route"
        };

        // ✅ Route → Required Role mapping (Admin-only routes)
        private static readonly Dictionary<string, string> _routeRoles = new(StringComparer.OrdinalIgnoreCase)
        {
            { "/api/employee/add",                          "Admin" },
            { "/api/employee/update",                       "Admin" },
            { "/api/employee/update-hiredate",              "Admin" },
            { "/api/employee/updateemployeestatus",         "Admin" },
            { "/api/employee/getallemployees",              "Admin" },
            { "/api/employee/createdesignation",            "Admin" },
            { "/api/employee/assigndesignation",            "Admin" },
            { "/api/employee/update-status",                "Admin" },
            { "/api/employee/getemployeeswithpendingleaves","Admin" },
            { "/api/employee/define",                       "Admin" },
            { "/api/employee/addaddress",                   "Admin" },
            { "/api/employee/addproject",                   "Admin" },
            { "/api/employee/assignemployeetoproject",      "Admin" },
            { "/api/employee/update-recordsforemployee",    "Admin" },
            { "/api/employee/payroll/create",               "Admin" },
          
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

            // ✅ Skip Swagger and health check routes
            if (path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/health", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            // ✅ Skip explicitly public routes
            if (_publicRoutes.Contains(path))
            {
                await _next(context);
                return;
            }

            // ✅ Determine if this route has a required role
            // Match route path (strip query string)
            var cleanPath = path.Split('?')[0].ToLower();
            _routeRoles.TryGetValue(cleanPath, out var requiredRole);

            // ✅ If no role required for this route, just pass through
            if (requiredRole == null)
            {
                await _next(context);
                return;
            }

            // ─── From here: this route REQUIRES a role ───────────────────

            // Step 1: Extract the token from Authorization header
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("🚫 Request to {Path} rejected — No Authorization header", path);
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new
                {
                    Success = false,
                    Message = "Unauthorized: No token provided. Please login first."
                });
                return;
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();

            // Step 2: Validate the token and extract claims
            ClaimsPrincipal? principal = ValidateToken(token);

            if (principal == null)
            {
                _logger.LogWarning("🚫 Request to {Path} rejected — Invalid or expired token", path);
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new
                {
                    Success = false,
                    Message = "Unauthorized: Token is invalid or has expired. Please login again."
                });
                return;
            }

            // Step 3: Check if the user's role matches the required role
            var userRole = principal.FindFirst(ClaimTypes.Role)?.Value;
            var userName = principal.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(userRole) ||
                !userRole.Equals(requiredRole, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning(
                    "🚫 User '{Username}' with role '{Role}' tried to access '{Path}' — Required role: '{RequiredRole}'",
                    userName, userRole ?? "None", path, requiredRole);

                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new
                {
                    Success = false,
                    Message = $"Forbidden: You do not have access to this resource. Required role: '{requiredRole}'."
                });
                return;
            }

            // ✅ Token is valid and role matches — attach user to context and continue
            context.User = principal;
            _logger.LogInformation("✅ User '{Username}' (Role: {Role}) accessed '{Path}'", userName, userRole, path);

            await _next(context);
        }

        /// <summary>
        /// Validates the JWT token and returns the ClaimsPrincipal if valid, or null if invalid.
        /// </summary>
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
                    ClockSkew = TimeSpan.Zero  // No tolerance for token expiry
                };

                var handler = new JwtSecurityTokenHandler();
                var principal = handler.ValidateToken(token, validationParams, out _);
                return principal;
            }
            catch (SecurityTokenExpiredException)
            {
                _logger.LogWarning("Token has expired.");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Token validation failed: {Message}", ex.Message);
                return null;
            }
        }
    }

    // ✅ Extension method for clean registration in Program.cs
    public static class JwtRoleMiddlewareExtensions
    {
        public static IApplicationBuilder UseJwtRoleMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<JwtRoleMiddleware>();
        }
    }
}
