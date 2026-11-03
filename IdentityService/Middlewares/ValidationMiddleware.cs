using FluentValidation;
using FluentValidation.Results;
using IdentityService.Domain;

namespace IdentityService.Middlewares
{
    public class ValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public ValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context,
                  IValidator<RegisterUserRequestDto> registerValidator,
                  IValidator<AdminLoginRequestDto> adminLoginValidator)
        {
            if (context.Request.Path.StartsWithSegments("/register"))
            {
                context.Request.EnableBuffering();
                var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
                context.Request.Body.Position = 0;

                var dto = System.Text.Json.JsonSerializer.Deserialize<RegisterUserRequestDto>(body);
                var result = await registerValidator.ValidateAsync(dto);

                if (!result.IsValid)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsJsonAsync(result.Errors.Select(e => new
                    {
                        e.PropertyName,
                        e.ErrorMessage
                    }));
                    return;
                }
            }
            else if (context.Request.Path.StartsWithSegments("/AdminLogin"))
            {
                context.Request.EnableBuffering();
                var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
                context.Request.Body.Position = 0;

                var dto = System.Text.Json.JsonSerializer.Deserialize<AdminLoginRequestDto>(body);
                var result = await adminLoginValidator.ValidateAsync(dto);

                if (!result.IsValid)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsJsonAsync(result.Errors.Select(e => new
                    {
                        e.PropertyName,
                        e.ErrorMessage
                    }));
                    return;
                }
            }

            await _next(context);
        }

    }

}
