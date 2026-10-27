using SkillHub.Application.Common.Models;

namespace SkillHub.Application.Common.Interfaces;

public interface IEnrollmentExcelParser
{
    IReadOnlyList<ImportRow> Parse(Stream stream);
}
