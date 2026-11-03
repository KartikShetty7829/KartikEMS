using EmployeeServiceDepartmentService.Exceptions;
using EmployeeServiceDepartmentService.Models;
using System.Text.Json;

namespace EmployeeServiceDepartmentService.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context); // Pass to the next middleware
            }
            catch (Exception ex)
            {
                // Log context: Request Method and Path
                var requestPath = context.Request.Path;
                var requestMethod = context.Request.Method;
                _logger.LogError(ex, $"Something went wrong while processing {requestMethod} {requestPath}: {ex.Message}");
                
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";

            // Determine the status code based on exception type
            context.Response.StatusCode = ex switch
            {
                NotFoundException => StatusCodes.Status404NotFound,
                ValidationException => StatusCodes.Status400BadRequest,
                BadRequestException => StatusCodes.Status400BadRequest,
                UnauthorizedException => StatusCodes.Status401Unauthorized,
                _ => StatusCodes.Status500InternalServerError
            };

            var errorDetails = new ErrorDetails
            {
                StatusCode = context.Response.StatusCode,
                Message = ex switch
                {
                    NotFoundException or ValidationException or BadRequestException or UnauthorizedException => ex.Message,
                    _ => "An unexpected error occurred."
                },
                StackTrace = _env.IsDevelopment() ? ex.StackTrace : null
            };

            var result = JsonSerializer.Serialize(errorDetails);
            return context.Response.WriteAsync(result);
        }
    }
}
