using System.Collections.Generic;

namespace System.CommandLine.JackFruit
{
    public interface ICommandProvider<TSource>
    {
        Command GetCommand(TSource source);

        // It's a little weird to have these details used by the method above
        // but it was sort of turning into a type explosion
        IHelpProvider<TSource> HelpProvider { get; set; }
        IEnumerable<Option> GetOptions(TSource source);
        IEnumerable<Command> GetSubCommands(TSource source);
        Argument GetArgument(TSource source);
        string GetName(TSource source);
        string GetHelp(TSource source);
    }
}
