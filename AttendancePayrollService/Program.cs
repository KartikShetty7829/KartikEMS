using AttendancePayrollService.DataAccess;
using AttendancePayrollService.ExternalService;
using AttendancePayrollService.Interfaces;
using AttendancePayrollService.Middlewares;
using AttendancePayrollService.repos;
using AttendanceService.Businesshandler;

using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Reflection;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);


// ✅ CORS – allow frontend app
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularApp", policy =>
    {
        policy.WithOrigins(
            "http://localhost:4200",                   // local dev
            "https://your-angular-app.azurewebsites.net" // Azure frontend
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});


// ✅ Configure Serilog safely
var logDir = Path.Combine(Directory.GetCurrentDirectory(), "logger");
try
{
    if (!Directory.Exists(logDir))
    {
        Directory.CreateDirectory(logDir);
    }
}
catch { }

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()                // log to console
    .WriteTo.File(Path.Combine(logDir, "log.txt"), rollingInterval: RollingInterval.Day, shared: true) // log to file
    .CreateLogger();

// Replace default logging with Serilog
builder.Host.UseSerilog();

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ Rate Limiting (ASP.NET Core 8 Native RateLimiter)
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Custom response on rate limit rejection
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/json";

        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
            await context.HttpContext.Response.WriteAsJsonAsync(new
            {
                Status = 429,
                Message = $"Rate limit exceeded. Try again in {(int)retryAfter.TotalSeconds} seconds.",
                RetryAfterSeconds = (int)retryAfter.TotalSeconds
            }, cancellationToken: cancellationToken);
        }
        else
        {
            await context.HttpContext.Response.WriteAsJsonAsync(new
            {
                Status = 429,
                Message = "Rate limit exceeded. Please try again after some time."
            }, cancellationToken: cancellationToken);
        }
    };

    // Attendance Policy: 30 requests per 1 minute per IP
    options.AddPolicy("AttendancePolicy", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                ?? httpContext.Connection.RemoteIpAddress?.ToString()
                ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
});

// ✅ Register DbContext with connection string from appsettings.json
builder.Services.AddDbContext<EmpAttendanceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ Register generic repository
//builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>));
builder.Services.AddScoped(typeof(IUnitOfWork<>), typeof(UnitOfWork<>));

// ✅ Register MediatR (scan assemblies for handlers)
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblies(
        Assembly.GetExecutingAssembly()
    ));

// ✅ Register IEmployeeServiceClient with HttpClient
builder.Services.AddHttpClient<IEmployeeServiceClient, EmployeeServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["EmployeeService:BaseUrl"]);
});


var app = builder.Build();

// ✅ Middleware order
app.UseCors("AngularApp");
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseStaticFiles();


// Middleware
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// ✅ Rate Limiter Middleware (after routing and auth)
app.UseRateLimiter();

app.MapControllers();

// ✅ Auto-initialize database schema if database is reachable
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<EmpAttendanceDbContext>();
        db.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Could not initialize database schema on startup. Verify database connection string.");
    }
}

app.Run();
