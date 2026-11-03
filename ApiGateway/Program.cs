using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Load Ocelot configuration file (supports environment override in Production)
builder.Configuration
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Register Ocelot
builder.Services.AddOcelot();

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Swagger for Gateway itself
builder.Services.AddControllers(); // <-- Add this line
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS policy for Angular frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularApp", policy =>
    {
        policy.WithOrigins(
            "http://localhost:4200",                   // Angular local dev server
            "https://your-angular-app.azurewebsites.net" // Angular Azure App Service
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

var app = builder.Build();

// Swagger UI (enable in dev + prod)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ApiGateway v1");
});

// Apply CORS before Ocelot
app.UseCors("AngularApp");

app.UseHttpsRedirection();
app.UseAuthorization();

// Map controllers so Swagger can discover endpoints
app.MapControllers();

// Ocelot must be the last middleware
await app.UseOcelot();

app.Run();
