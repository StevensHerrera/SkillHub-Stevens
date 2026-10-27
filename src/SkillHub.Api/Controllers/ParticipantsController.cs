using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.Dtos.Participants;
using SkillHub.Application.Services;

namespace SkillHub.Api.Controllers;

[ApiController]
[Route("api/v1/participants")]
[Authorize(Policy = "Employee")]
public sealed class ParticipantsController(ParticipantService participantService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ParticipantDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ParticipantDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await participantService.GetAllAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ParticipantDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ParticipantDto>> GetById(Guid id, CancellationToken cancellationToken) =>
        Ok(await participantService.GetByIdAsync(id, cancellationToken));

    [HttpPost]
    [ProducesResponseType(typeof(ParticipantDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ParticipantDto>> Create(
        CreateParticipantRequest request,
        CancellationToken cancellationToken)
    {
        var created = await participantService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ParticipantDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ParticipantDto>> Update(
        Guid id,
        UpdateParticipantRequest request,
        CancellationToken cancellationToken) =>
        Ok(await participantService.UpdateAsync(id, request, cancellationToken));
}
