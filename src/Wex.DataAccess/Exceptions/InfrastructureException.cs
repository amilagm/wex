using Wex.Domain.Exceptions;

namespace Wex.DataAccess.Exceptions;

public class InfrastructureException : WexException
{
    public InfrastructureException(string message) : base(message) { }
    public InfrastructureException(string message, Exception innerException) : base(message, innerException) { }
}
