using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.Dtos.Reports;
using SkillHub.Application.Services;

namespace SkillHub.Api.Controllers;

[ApiController]
[Route("api/v1/reports")]
public sealed class ReportsController(ReportService reportService) : ControllerBase
{
    [HttpGet("top-workshops")]
    [Authorize(Policy = "Employee")]
    [ProducesResponseType(typeof(IReadOnlyList<TopWorkshopDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TopWorkshopDto>>> GetTopWorkshops(
        CancellationToken cancellationToken) =>
        Ok(await reportService.GetTopWorkshopsAsync(cancellationToken));
}
