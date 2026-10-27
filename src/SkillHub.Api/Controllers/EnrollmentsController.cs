using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.Dtos.Enrollments;
using SkillHub.Application.Services;

namespace SkillHub.Api.Controllers;

[ApiController]
[Route("api/v1/enrollments")]
public sealed class EnrollmentsController(EnrollmentService enrollmentService) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = "Participant")]
    [ProducesResponseType(typeof(EnrollmentDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<EnrollmentDto>> Create(
        CreateEnrollmentRequest request,
        CancellationToken cancellationToken)
    {
        var created = await enrollmentService.EnrollAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, created);
    }

    [HttpGet("mine")]
    [Authorize(Policy = "Participant")]
    [ProducesResponseType(typeof(IReadOnlyList<EnrollmentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<EnrollmentDto>>> GetMine(CancellationToken cancellationToken) =>
        Ok(await enrollmentService.GetMineAsync(cancellationToken));

    [HttpPatch("{id:guid}/cancel")]
    [Authorize]
    [ProducesResponseType(typeof(EnrollmentDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<EnrollmentDto>> Cancel(
        Guid id,
        CancelEnrollmentRequest request,
        CancellationToken cancellationToken) =>
        Ok(await enrollmentService.CancelAsync(id, request, cancellationToken));

    [HttpPost("import")]
    [Authorize(Policy = "Employee")]
    [RequestSizeLimit(10_000_000)]
    [ProducesResponseType(typeof(ImportEnrollmentsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ImportEnrollmentsResponse>> Import(
        IFormFile? file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation error",
                Detail = "An Excel file is required in the 'file' form field."
            });

        await using var stream = file.OpenReadStream();
        return Ok(await enrollmentService.ImportAsync(stream, cancellationToken));
    }

    [HttpGet("{id:guid}/certificate")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Certificate(Guid id, CancellationToken cancellationToken)
    {
        var (content, fileName) = await enrollmentService.GetCertificateAsync(id, cancellationToken);
        return File(content, "application/pdf", fileName);
    }
}
