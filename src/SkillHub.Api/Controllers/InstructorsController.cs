using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.Dtos.Instructors;
using SkillHub.Application.Services;

namespace SkillHub.Api.Controllers;

[ApiController]
[Route("api/v1/instructors")]
[Authorize(Policy = "Employee")]
public sealed class InstructorsController(InstructorService instructorService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<InstructorDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<InstructorDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await instructorService.GetAllAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(InstructorDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<InstructorDto>> GetById(Guid id, CancellationToken cancellationToken) =>
        Ok(await instructorService.GetByIdAsync(id, cancellationToken));

    [HttpPost]
    [ProducesResponseType(typeof(InstructorDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<InstructorDto>> Create(
        CreateInstructorRequest request,
        CancellationToken cancellationToken)
    {
        var created = await instructorService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(InstructorDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<InstructorDto>> Update(
        Guid id,
        UpdateInstructorRequest request,
        CancellationToken cancellationToken) =>
        Ok(await instructorService.UpdateAsync(id, request, cancellationToken));
}
