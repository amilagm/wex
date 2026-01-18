using System.CommandLine;

namespace Wex.Cli.Commands;

public interface ICliCommand
{
    Command Build();
}
