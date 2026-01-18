using System.CommandLine;
using System.CommandLine.Parsing;
using Wex.Domain.Exceptions;

namespace Wex.Cli.Commands;

public static class CommandHelper
{
    public static void SetHandledAction(this Command command, Func<ParseResult, CancellationToken, Task> action)
    {
        command.SetAction(async (parseResult, ct) =>
        {
            try
            {
                await action(parseResult, ct);
                return 0;
            }
            catch (WexException ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                return 1;
            }
        });
    }
}
