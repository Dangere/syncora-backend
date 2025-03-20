using System.Text.Json;

class ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            //Process request before passing to the next middleware
            await _next(context);
        }
        catch (Exception ex)
        {
            //Process error after an exception is thrown
            _logger.LogError(ex, "An error occurred!");

            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            var errorResponse = new { Message = "An error occurred." };
            await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
        }
    }
}