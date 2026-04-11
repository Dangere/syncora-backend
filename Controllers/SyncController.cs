using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SyncoraBackend.Attributes;
using SyncoraBackend.Enums;
using SyncoraBackend.Models.DTOs.Sync;
using SyncoraBackend.Services;
using SyncoraBackend.Utilities;

namespace SyncoraBackend.Controllers;

[AuthorizeRoles(UserRoles.User, UserRoles.Admin)]
[ApiController]
[Route("api/sync")]
public class SyncController(ClientSyncService syncService) : ControllerBase
{
    private readonly ClientSyncService _syncService = syncService;

    [HttpGet("{since}")]
    public async Task<IActionResult> SyncSince([FromRoute] string since, [FromQuery] bool includeDeleted = false)
    {

        if (!DateTime.TryParse(since, out DateTime formattedSince))
        {
            return BadRequest("Invalid date format.");

        }

        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        Result<SyncPayload> dataResult = await _syncService.SyncSinceTimestamp(userId, formattedSince, includeDeleted);

        if (!dataResult.IsSuccess)
            return this.ErrorResponse(dataResult);

        return Ok(dataResult.Data);
    }

}