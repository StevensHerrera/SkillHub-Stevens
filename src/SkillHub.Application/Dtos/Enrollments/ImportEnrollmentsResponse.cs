namespace SkillHub.Application.Dtos.Enrollments;

public sealed record ImportRowError(int Row, string Reason);

public sealed record ImportEnrollmentsResponse(
    int RowsRead,
    int Enrolled,
    int Failed,
    IReadOnlyList<ImportRowError> Errors);
