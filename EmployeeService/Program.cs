using Asp.Versioning;
using EmployeeServiceDepartmentService.data_access;
using EmployeeServiceDepartmentService.emailservicefolder;
using EmployeeServiceDepartmentService.External_Service;
using EmployeeServiceDepartmentService.interfaces;
using EmployeeServiceDepartmentService.MessageServices;
using EmployeeServiceDepartmentService.Middlewares;
using EmployeeServiceDepartmentService.repos;
using EmployeeServiceDepartmentService.Services;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using Serilog;
using System.Reflection;
using System.Text;
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

// ✅ Serilog logging safely
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
    .WriteTo.Console()
    .WriteTo.File(Path.Combine(logDir, "log.txt"), rollingInterval: RollingInterval.Day, shared: true)
    .CreateLogger();

builder.Host.UseSerilog();

// ✅ API Versioning (URL Versioning)
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new Asp.Versioning.UrlSegmentApiVersionReader();
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

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

    // Notification Policy: 3 requests per 1 minute (Twilio SMS & SMTP Email triggers)
    options.AddPolicy("NotificationPolicy", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.IsAuthenticated == true 
                ? (httpContext.User.Identity.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "auth_user")
                : (httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous"),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 3,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    // Payslip Policy: 5 requests per 1 minute (heavy PDF generation & DB queries)
    options.AddPolicy("PayslipPolicy", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.IsAuthenticated == true 
                ? (httpContext.User.Identity.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "auth_user")
                : (httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous"),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    // General Employee CRUD Policy: 60 requests per 1 minute
    options.AddPolicy("EmployeeGeneralPolicy", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                ?? httpContext.Connection.RemoteIpAddress?.ToString()
                ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 60,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
});

// ✅ Controllers
builder.Services.AddControllers();

// ✅ DbContext
builder.Services.AddDbContext<EmpDepartmentDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ Repository & UoW
builder.Services.AddScoped(typeof(IReadOnlyRepository<,>), typeof(ReadOnlyRepository<,>));
builder.Services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>));
builder.Services.AddScoped(typeof(IUnitOfWork<>), typeof(UnitOfWork<>));

// ✅ MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));

// ✅ Email & Message Services
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddTransient<IEmailService, EmailService>();

builder.Services.Configure<TwilioSettings>(builder.Configuration.GetSection("TwilioSettings"));
builder.Services.AddTransient<IMessageService, MessageService>();

// ✅ Payslip Service
builder.Services.AddScoped<IPayslipService, PayslipService>();

// ✅ Kafka Services
builder.Services.AddSingleton<IKafkaProducerService, KafkaProducerService>();
builder.Services.AddHostedService<NotificationConsumerService>();

builder.WebHost.UseWebRoot("wwwroot");
try
{
    var webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
    var uploadsDir = Path.Combine(webRoot, "uploads", "profile_pics");
    var payslipsDir = Path.Combine(webRoot, "payslips");
    if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);
    if (!Directory.Exists(payslipsDir)) Directory.CreateDirectory(payslipsDir);
}
catch { }

// ✅ HttpClient for external UserService
builder.Services.AddHttpClient<UserServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["UserService:BaseUrl"]);
});

// ✅ JWT Authentication
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // true in production
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSection["Issuer"],
        ValidAudience = jwtSection["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// ✅ Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOrEmployee", policy =>
        policy.RequireAssertion(context =>
            context.User.IsInRole("Admin") || context.User.IsInRole("Employee")));
});

// ✅ Swagger with JWT Bearer
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "EmployeeService API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token here. Example: Bearer {your_token}"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
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

// ✅ Authentication before Authorization
app.UseAuthentication();
app.UseAuthorization();

// ✅ Custom JWT Role Middleware
app.UseJwtRoleMiddleware();

// ✅ Rate Limiter Middleware (after routing and authentication)
app.UseRateLimiter();

app.MapControllers();

// ✅ Auto-initialize database schema if database is reachable
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<EmpDepartmentDbContext>();
        db.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Could not initialize database schema on startup. Verify database connection string.");
    }
}

app.Run();
