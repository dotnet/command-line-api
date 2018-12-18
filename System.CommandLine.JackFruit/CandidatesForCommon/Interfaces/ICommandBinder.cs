using System.Collections.Generic;

namespace System.CommandLine.JackFruit
{
    public interface ICommandBinder<TCommandSource, TOptionSource>
    {
        Command GetCommand(TCommandSource source);

        IEnumerable<TOptionSource> GetOptionSources(TCommandSource source);
        IEnumerable<TCommandSource> GetSubCommandSources(TCommandSource source);

        Argument GetArgument(TCommandSource source);
        string GetHelp(TCommandSource source);
    }
}
