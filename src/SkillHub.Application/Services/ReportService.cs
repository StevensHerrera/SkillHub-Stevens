using SkillHub.Application.Common.Exceptions;
using SkillHub.Application.Common.Interfaces;
using SkillHub.Application.Common.Mappings;
using SkillHub.Application.Dtos.Reports;

namespace SkillHub.Application.Services;

public sealed class ReportService(
    IWorkshopRepository workshops,
    ICurrentUserService currentUser)
{
    public async Task<IReadOnlyList<TopWorkshopDto>> GetTopWorkshopsAsync(CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsEmployee)
            throw new ForbiddenException("Only SkillHub employees can consult this report.");

        var items = await workshops.GetTopAsync(cancellationToken);
        return items.Select(item => item.ToTopDto()).ToArray();
    }
}
