using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SyncoraBackend.Attributes;
using SyncoraBackend.Enums;
using SyncoraBackend.Models.DTOs.Report;
using SyncoraBackend.Services;
using SyncoraBackend.Utilities;

namespace SyncoraBackend.Controllers;



[AuthorizeRoles(UserRoles.Admin, UserRoles.User)]
[ApiController]
[Route("api/[controller]")]
public class ReportController(ReportServices reportService) : ControllerBase
{

    private readonly ReportServices _reportService = reportService;
    [HttpPost("error")]
    public async Task<IActionResult> SubmitErrorReport([FromBody] SubmitReportDTO submitReportDTO)
    {
        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        Result<string> result = await _reportService.StoreErrorReport(submitReportDTO, userId);

        if (!result.IsSuccess)
            return this.ErrorResponse(result);

        return Ok();


    }

    [HttpPost("bug")]
    public async Task<IActionResult> SubmitBugReport([FromBody] SubmitReportDTO submitReportDTO)
    {
        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        Result<string> result = await _reportService.StoreBugReport(submitReportDTO, userId);

        if (!result.IsSuccess)
            return this.ErrorResponse(result);

        return Ok();


    }

}
