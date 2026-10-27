namespace SkillHub.Domain.Common;

public enum DomainErrorKind
{
    Validation = 0,
    Conflict = 1,
    Forbidden = 2,
    NotFound = 3
}

public sealed class DomainException : Exception
{
    public DomainException(string message, DomainErrorKind kind = DomainErrorKind.Validation)
        : base(message)
    {
        Kind = kind;
    }

    public DomainErrorKind Kind { get; }
}
