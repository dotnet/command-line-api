using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace System.CommandLine.JackFruit
{
    // This is the working class until things become functional and then 
    // refactoring into general CommandBinder and creation of MethodBinder
    // JsonBinder and SuperBinder (T non-leaf, method leaf, and yes will 
    // give it another name. 
    public abstract class CommandBinder<TCommandBinder, TCommandSource, TOptionSource>
        : ICommandBinder<TCommandSource, TOptionSource>
        where TCommandBinder : CommandBinder<TCommandBinder, TCommandSource, TOptionSource>
    {
        // KAD: Use helpProvider, Add a name provider,
        private protected readonly IHelpProvider<TCommandSource> helpProvider;
        private protected readonly IOptionBinder<TCommandSource, TOptionSource> optionProvider;
        private protected readonly IArgumentBinder<TCommandSource, TOptionSource> argumentProvider;
        private protected readonly IInvocationProvider invocationProvider;
        private protected readonly bool shouldRemoveParentNames;
        private Stack<(string Raw, string Munged)> parentNames;

        protected CommandBinder(
                    IDescriptionProvider<TCommandSource> descriptionProvider = null,
                    IHelpProvider<TCommandSource> helpProvider = null,
                    IOptionBinder<TCommandSource, TOptionSource> optionProvider = null,
                    IArgumentBinder<TCommandSource, TOptionSource> argumentProvider = null,
                    IInvocationProvider invocationProvider = null,
                    bool shouldRemoveParentNames = false)
        {
            this.helpProvider = helpProvider ?? new GeneralHelpProvider<TCommandSource>(descriptionProvider);
            this.optionProvider = optionProvider;
            this.argumentProvider = argumentProvider;
            this.invocationProvider = invocationProvider;

            this.optionProvider.HelpProvider = this.optionProvider.HelpProvider
                                ?? this.helpProvider;
            this.argumentProvider.HelpProvider = this.argumentProvider.HelpProvider
                                ?? this.helpProvider;
            parentNames = new Stack<(string Raw, string Munged)>();
            this.shouldRemoveParentNames = shouldRemoveParentNames;
        }

        public abstract string GetName(TCommandSource current);
        protected abstract void SetHandler(Command command, TCommandSource current);
        public abstract IEnumerable<TOptionSource> GetOptionSources(TCommandSource source);
        public abstract IEnumerable<TCommandSource> GetSubCommandSources(TCommandSource source);

        public Command GetCommand(TCommandSource current)
        {
            var name = GetName(current);
            var mungedName = RemoveParentNames(name);
            var useName = shouldRemoveParentNames
                            ? mungedName
                            : name;

            parentNames.Push((name, mungedName));
            var command = FillCommand(current, new Command(name: useName.ToKebabCase()));
            parentNames.Pop();

            return command;
        }

        private string RemoveParentNames(string candidate)
        {
            var reverseNames = parentNames.Reverse();
            foreach (var parentName in reverseNames)
            {
                if (candidate.StartsWith(parentName.Munged))
                {
                    candidate = candidate.Substring(parentName.Munged.Length);
                }
            }
            return candidate;
        }

        public Argument GetArgument(TCommandSource source)
            => argumentProvider.GetArgument(source);

        public RootCommand GetRootCommand(TCommandSource current)
             => FillCommand(current, new RootCommand());

        public IEnumerable<Option> GetOptions(TCommandSource current)
            => GetOptionSources(current)
                .Select(x => optionProvider.GetOption(current, x));

        public IEnumerable<Command> GetSubCommands(TCommandSource current)
            => GetSubCommandSources(current)
                    ?.Select(t => GetCommand(t));

        public string GetHelp(TCommandSource current)
            => helpProvider.GetHelp(current);

        public string GetHelp(TCommandSource current, TOptionSource optionSource)
            => helpProvider.GetHelp(current, optionSource);

        protected TCommand FillCommand<TCommand>(TCommandSource current, TCommand command)
                where TCommand : Command
        {
            command.Description = GetHelp(current);
            SetHandler(command, current);

            command.AddOptions(GetOptions(current));
            command.Argument = GetArgument(current);

            return command
                .AddCommands(GetSubCommands(current));
        }

    }
}
