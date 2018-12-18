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
    public abstract class CommandBinder<TCommandSource, TOptionSource> : ICommandBinder<TCommandSource, TOptionSource>
    {
        // KAD: Use OptionProvider, helpProvider, Add a name provider,
        private protected readonly IHelpProvider<TCommandSource, TOptionSource> helpProvider;
        private protected readonly IOptionBinder<TCommandSource, TOptionSource> optionProvider;
        private protected readonly IArgumentBinder<TCommandSource, TOptionSource> argumentProvider;
        private protected readonly IInvocationProvider invocationProvider;

        public CommandBinder(
                    IDescriptionProvider<TCommandSource> descriptionProvider = null,
                    IHelpProvider<TCommandSource, TOptionSource> helpProvider = null,
                    IOptionBinder<TCommandSource, TOptionSource> optionProvider = null,
                    IArgumentBinder<TCommandSource, TOptionSource> argumentProvider = null,
                    IInvocationProvider invocationProvider = null)
        {
            this.helpProvider = helpProvider;
            this.optionProvider = optionProvider;
            this.argumentProvider = argumentProvider;
            this.invocationProvider = invocationProvider;

            this.optionProvider.HelpProvider = this.optionProvider.HelpProvider
                                ?? this.helpProvider;
            this.argumentProvider.HelpProvider = this.argumentProvider.HelpProvider
                                ?? this.helpProvider;
        }

        public IHelpProvider<TCommandSource> HelpProvider { get; set; }

        public Argument GetArgument(TCommandSource source)
        {
            return argumentProvider.GetArgument(source);
        }

        public abstract Command GetCommand(TCommandSource current);

        public abstract RootCommand GetRootCommand(TCommandSource current);

        public abstract string GetHelp(TCommandSource current);

        public abstract string GetName(TCommandSource current);

        public IEnumerable<Option> GetOptions(TCommandSource current) 
            => GetOptionSources(current)
                .Where(p => argumentProvider.IsArgument(current, p))
                .Select(x => optionProvider.GetOption(current, x));

        public abstract IEnumerable<Command> GetSubCommands(TCommandSource current);

        public abstract IEnumerable<TOptionSource> GetOptionSources(TCommandSource source);
    }
}
