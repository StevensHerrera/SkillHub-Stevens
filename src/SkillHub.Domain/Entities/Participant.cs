using SkillHub.Domain.Common;

namespace SkillHub.Domain.Entities;

public sealed class Participant
{
    private Participant()
    {
    }

    public Participant(Guid userId, string fullName, string email)
    {
        if (userId == Guid.Empty) throw new DomainException("The participant user is required.");
        UserId = userId;
        FullName = Require(fullName, "Full name", 200);
        Email = Require(email, "Email", 256).ToLowerInvariant();
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    public void Update(string fullName, string email)
    {
        FullName = Require(fullName, "Full name", 200);
        Email = Require(email, "Email", 256).ToLowerInvariant();
    }

    private static string Require(string value, string field, int maxLength)
    {
        var trimmed = (value ?? string.Empty).Trim();
        if (trimmed.Length == 0) throw new DomainException($"{field} is required.");
        if (trimmed.Length > maxLength) throw new DomainException($"{field} cannot exceed {maxLength} characters.");
        return trimmed;
    }
}
