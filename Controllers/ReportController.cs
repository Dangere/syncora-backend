using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SyncoraBackend.Attributes;
using SyncoraBackend.Enums;
using SyncoraBackend.Models.DTOs.Report;
using SyncoraBackend.Services;
using SyncoraBackend.Utilities;

namespace SyncoraBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("report-policy")]
public class ReportController(ReportServices reportService) : ControllerBase
{

    private readonly ReportServices _reportService = reportService;
    [AllowAnonymous, HttpPost("error")]
    public async Task<IActionResult> SubmitErrorReport([FromBody] SubmitReportDTO submitReportDTO)
    {
        int? userId = int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int id) ? id : null;

        Result<string> result = await _reportService.StoreErrorReport(submitReportDTO, userId);

        if (!result.IsSuccess)
            return this.ErrorResponse(result);

        return Ok();


    }

    [AllowAnonymous, HttpPost("bug")]
    public async Task<IActionResult> SubmitBugReport([FromBody] SubmitReportDTO submitReportDTO)
    {
        int? userId = int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int id) ? id : null;


        Result<string> result = await _reportService.StoreBugReport(submitReportDTO, userId);

        if (!result.IsSuccess)
            return this.ErrorResponse(result);

        return Ok();


    }

}
