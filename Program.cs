using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TaskManagementWebAPI.Data;
using TaskManagementWebAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Register AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Add controllers, Lifecycle: Transient but behaves like scoped because it gets created per HTTP request
builder.Services.AddControllers();

// Add services, Lifecycle: Scoped
builder.Services.AddScoped<TaskService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserService>();

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

});

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlerMiddleware>();

app.UseRouting();
app.MapControllers();

// Using authentication and authorization middleware to secure endpoints
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "Hello World!");
app.Run();
