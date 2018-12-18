using System.Collections.Generic;

namespace System.CommandLine.JackFruit
{
    public interface ICommandBinder<TCommandSource, TOptionSource>
    {
        Command GetCommand(TCommandSource source);

        // It's a little weird to have these details used by the method above
        // but it was sort of turning into a type explosion
        IHelpProvider<TCommandSource> HelpProvider { get; set; }

        IEnumerable<TOptionSource> GetOptionSources(TCommandSource source);


        IEnumerable<Command> GetSubCommands(TCommandSource source);
        Argument GetArgument(TCommandSource source);
        string GetName(TCommandSource source);
        string GetHelp(TCommandSource source);
    }
}
