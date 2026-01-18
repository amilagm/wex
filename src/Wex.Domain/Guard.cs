using Wex.Domain.Exceptions;

namespace Wex.Domain;

public static class Guard
{
    public static string NotNullOrWhiteSpace(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"{parameterName} is required.");
        }
        return value;
    }

    public static string MaxLength(string value, int maxLength, string parameterName)
    {
        if (value.Length > maxLength)
        {
            throw new DomainException($"{parameterName} must not exceed {maxLength} characters.");
        }
        return value;
    }

    public static Guid NotEmpty(Guid value, string parameterName)
    {
        if (value == Guid.Empty)
        {
            throw new DomainException($"{parameterName} cannot be empty.");
        }
        return value;
    }
}
