namespace Wex.Domain.Exceptions;

public abstract class WexException : Exception
{
    protected WexException(string message) : base(message) { }
    protected WexException(string message, Exception innerException) : base(message, innerException) { }
}

public class DomainException(string message) : WexException(message)
{
}

public class NotFoundException(string message) : WexException(message)
{
}

public class ConflictException(string message) : WexException(message)
{
}

public class ConversionException : WexException
{
    public ConversionException(string message) : base(message) { }
    public ConversionException(string message, Exception innerException) : base(message, innerException) { }
}
