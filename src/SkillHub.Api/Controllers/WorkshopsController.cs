using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.Dtos.Enrollments;
using SkillHub.Application.Dtos.Workshops;
using SkillHub.Application.Services;

namespace SkillHub.Api.Controllers;

[ApiController]
[Route("api/v1/workshops")]
public sealed class WorkshopsController(WorkshopService workshopService, EnrollmentService enrollmentService)
    : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<WorkshopDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<WorkshopDto>>> GetCatalog(CancellationToken cancellationToken) =>
        Ok(await workshopService.GetCatalogAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(WorkshopDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<WorkshopDto>> GetById(Guid id, CancellationToken cancellationToken) =>
        Ok(await workshopService.GetByIdAsync(id, cancellationToken));

    [HttpPost]
    [Authorize(Policy = "Employee")]
    [ProducesResponseType(typeof(WorkshopDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<WorkshopDto>> Create(
        CreateWorkshopRequest request,
        CancellationToken cancellationToken)
    {
        var created = await workshopService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "Employee")]
    [ProducesResponseType(typeof(WorkshopDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<WorkshopDto>> Update(
        Guid id,
        UpdateWorkshopRequest request,
        CancellationToken cancellationToken) =>
        Ok(await workshopService.UpdateAsync(id, request, cancellationToken));

    [HttpGet("{id:guid}/enrollments")]
    [Authorize(Policy = "Employee")]
    [ProducesResponseType(typeof(IReadOnlyList<WorkshopEnrollmentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<WorkshopEnrollmentDto>>> GetEnrollments(
        Guid id,
        CancellationToken cancellationToken) =>
        Ok(await enrollmentService.GetWorkshopEnrollmentsAsync(id, cancellationToken));
}
