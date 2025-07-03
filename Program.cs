using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SyncoraBackend.Data;
using SyncoraBackend.Hubs;
using SyncoraBackend.Services;

var builder = WebApplication.CreateBuilder(args);

// Register AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Add controllers, Lifecycle: Transient but behaves like scoped because it gets created per HTTP request
builder.Services.AddControllers();

builder.Services.AddSignalR();

// Add services, Lifecycle: Scoped
builder.Services.AddScoped<TaskService>();
builder.Services.AddScoped<GroupService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UsersService>();
builder.Services.AddScoped<ClientSyncService>();
builder.Services.AddSingleton<SyncHub>();
builder.Services.AddSingleton<NotificationHub>();


// Add DbContext to the services, Lifecycle: Scoped
builder.Services.AddDbContext<SyncoraDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(jwtOptions =>
{
    var jwtConfig = builder.Configuration.GetSection("Jwt");
    SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(jwtConfig["SecretKey"]!));

    jwtOptions.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,  // Ensure the token is from a trusted issuer
        ValidateAudience = true,  // Ensure the token is intended for the correct audience
        ValidateLifetime = true,  // Ensure the token hasn't expired
        ValidateIssuerSigningKey = true,  // Validate the signature to ensure integrity

        ValidIssuer = jwtConfig["Issuer"],  // Expected issuer
        ValidAudience = jwtConfig["Audience"],  // Expected audience
        IssuerSigningKey = key  // Use the stored secret key
    };

    // We have to hook the OnMessageReceived event in order to
    // allow the JWT authentication handler to read the access
    // token from the query string when a WebSocket or 
    // Server-Sent Events request comes in.

    // Sending the access token in the query string is required when using WebSockets or ServerSentEvents
    // due to a limitation in Browser APIs. We restrict it to only calls to the
    // SignalR hub in this code.
    // See https://docs.microsoft.com/aspnet/core/signalr/security#access-token-logging
    // for more information about security considerations when using
    // the query string to transmit the access token.
    jwtOptions.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];

            // If the request is for our hub...
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) &&
                (path.StartsWithSegments("/hubs/sync")))
            {
                // Read the token out of the query string
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };

});

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        string clientUsername = httpContext.User.FindFirstValue(ClaimTypes.Name) ?? httpContext.Request.Headers.Host.ToString();

        return RateLimitPartition.GetFixedWindowLimiter(partitionKey: clientUsername, factory: _ =>
        {
            return new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100, // Max 100 requests
                Window = TimeSpan.FromMinutes(1), // Per 1 minute window
                QueueLimit = 0, // No queuing, immediate rejection if limit exceeded
            };
        });

    });

    options.AddPolicy("auth-policy", httpContext =>
    {
        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(partitionKey: clientIp, factory: _ =>
        {
            return new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10, // Max 10 requests
                Window = TimeSpan.FromMinutes(1), // Per 1 minute window
                QueueLimit = 0, // No queuing, immediate rejection if limit exceeded
            };
        });
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlerMiddleware>();

app.UseRouting();
app.UseRateLimiter();
app.MapControllers();

// Using authentication and authorization middleware to secure endpoints
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<SyncHub>("/hubs/sync");
app.MapHub<SyncHub>("/hubs/notification");

app.MapGet("/", () => "Hello World!");
app.Run();

public partial class Program { }