using System.CommandLine;

namespace Wex.Cli.Commands;

public class CommandBuilder(IEnumerable<ICliCommand> commands)
{
    public RootCommand Build()
    {
        var root = new RootCommand("Wex - Credit card and purchase management CLI");
        foreach (var command in commands)
        {
            root.Add(command.Build());
        }
        return root;
    }
}
