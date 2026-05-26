using System.Security.Claims;
using System.Text.Json;
using SyncoraBackend.Models;

class UserContextMiddleware(RequestDelegate next, ILogger<UserContextMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<UserContextMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context, UserRequestContext requestContext)
    {
        // Letting preflight pass through to the CORS middleware
        if (context.Request.Method == HttpMethods.Options)
        {
            await _next(context);
            return;
        }
        // Letting the hub pass through because we extract the data on the hub
        if (context.Request.Path.StartsWithSegments("/hubs"))
        {
            await _next(context);
            return;
        }

        bool hasUserId = int.TryParse(context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId);

        // Letting anonymous users pass through
        if (!hasUserId)
        {
            await _next(context);
            return;
        }

        bool hasDeviceIdHeader = context.Request.Headers.TryGetValue("Device-Id", out var deviceId);
        if (!hasDeviceIdHeader)
        {
            context.Response.StatusCode = 400;
            context.Response.ContentType = "application/json";
            var errorResponse = new { Message = "Device-Id header is missing" };
            await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
            return;
        }



        requestContext.PopulateContext(userId, deviceId!);

        await _next(context);
    }
}