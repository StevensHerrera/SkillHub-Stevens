using SkillHub.Domain.Common;

namespace SkillHub.Domain.Entities;

public sealed class Instructor
{
    private Instructor()
    {
    }

    public Instructor(Guid userId, string fullName, string email, string specialty)
    {
        if (userId == Guid.Empty) throw new DomainException("The instructor user is required.");
        UserId = userId;
        FullName = Require(fullName, "Full name", 200);
        Email = Require(email, "Email", 256).ToLowerInvariant();
        Specialty = Require(specialty, "Specialty", 150);
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Specialty { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    public void Update(string fullName, string email, string specialty)
    {
        FullName = Require(fullName, "Full name", 200);
        Email = Require(email, "Email", 256).ToLowerInvariant();
        Specialty = Require(specialty, "Specialty", 150);
    }

    private static string Require(string value, string field, int maxLength)
    {
        var trimmed = (value ?? string.Empty).Trim();
        if (trimmed.Length == 0) throw new DomainException($"{field} is required.");
        if (trimmed.Length > maxLength) throw new DomainException($"{field} cannot exceed {maxLength} characters.");
        return trimmed;
    }
}
