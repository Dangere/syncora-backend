using Microsoft.AspNetCore.Mvc;
using SyncoraBackend.Utilities;

namespace SyncoraBackend.Utilities;

public static class ControllerExtensions
{
    /// <summary>
    ///    Controller extension that generates an error response to be sent to the client directly from the centralized Result class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="controller"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public static IActionResult ErrorResponse<T>(this ControllerBase controller, Result<T> result)
    {
        return controller.StatusCode(result.ErrorStatusCode, new { message = result.ErrorMessage, code = result.ErrorCode });
    }
}
