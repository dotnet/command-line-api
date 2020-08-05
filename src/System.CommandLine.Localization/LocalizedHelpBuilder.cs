using System.CommandLine.Help;
using System.Linq;
using Microsoft.Extensions.Localization;

namespace System.CommandLine.Localization
{
    public class LocalizedHelpBuilder : HelpBuilder
    {
        private readonly IStringLocalizer localizer;

        public LocalizedHelpBuilder(IStringLocalizer localizer,
            IConsole console, int? columnGutter = null, 
            int? indentationSize = null, int? maxWidth = null)
            : base(console, columnGutter, indentationSize, maxWidth)
        {
            this.localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        }

        public override void Write(ICommand command)
        {
            base.Write(GetLocalizedCommand(command));
        }

        public override void Write(IOption option)
        {
            base.Write(GetLocalizedOption(option));
        }

        private Command GetLocalizedCommand(ICommand command)
        {
            var lcmd = new Command(command.Name);
            Localize(lcmd, command);

            foreach (IOption option in command.Options)
                lcmd.AddOption(GetLocalizedOption(option));
            foreach (IArgument argument in command.Arguments)
                lcmd.AddArgument(GetLocalizedArgument(argument));

            lcmd.TreatUnmatchedTokensAsErrors = command.TreatUnmatchedTokensAsErrors;

            return lcmd;
        }

        private Option GetLocalizedOption(IOption option)
        {
            var lopt = new Option(option.Aliases.First());
            Localize(lopt, option);

            lopt.Name = option.Name;
            lopt.Argument = GetLocalizedArgument(option.Argument);
            lopt.IsRequired = option.IsRequired;

            return lopt;
        }

        private Argument GetLocalizedArgument(IArgument argument)
        {
            var larg = new Argument(argument.Name);
            Localize(larg, argument);
            larg.ArgumentType = argument.ValueType;
            larg.Arity = argument.Arity;
            if (argument.HasDefaultValue)
                larg.SetDefaultValueFactory(() => argument.GetDefaultValue());
            larg.AddSuggestions(txtToMatch => argument.GetSuggestions(txtToMatch)!);

            return larg;
        }

        private void Localize(Symbol symbol, ISymbol source)
        {
            var ldesc = string.IsNullOrEmpty(source.Description)
                ? source.Description
                : localizer.GetString(source.Description);
            symbol.Description = ldesc;

            foreach (var alias in source.Aliases)
                symbol.AddAlias(alias);

            symbol.IsHidden = source.IsHidden;
        }
    }
}
