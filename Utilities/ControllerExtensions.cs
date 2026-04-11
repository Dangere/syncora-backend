using Microsoft.AspNetCore.Mvc;
using SyncoraBackend.Utilities;

namespace SyncoraBackend.Utilities;

public static class ControllerExtensions
{
    public static IActionResult ErrorResponse<T>(this ControllerBase controller, Result<T> result)
    {
        return controller.StatusCode(result.ErrorStatusCode, new { message = result.ErrorMessage, code = result.ErrorCode });
    }
}
