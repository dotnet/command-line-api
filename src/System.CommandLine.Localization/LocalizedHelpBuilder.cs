using System.CommandLine.Help;
using System.Linq;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace System.CommandLine.Localization
{
    public class LocalizedHelpBuilder : HelpBuilder
    {
        private readonly IStringLocalizer localizer;
        private readonly IStringLocalizer helpLocalizer;

        public LocalizedHelpBuilder(IStringLocalizerFactory localizerFactory,
            Type resourceSource, IConsole console, int? columnGutter = null,
            int? indentationSize = null, int? maxWidth = null)
            : base(console, columnGutter, indentationSize, maxWidth)
        {
            localizerFactory ??= new ResourceManagerStringLocalizerFactory(
                Options.Create(new LocalizationOptions()), NullLoggerFactory.Instance);

            localizer = localizerFactory.Create(resourceSource);
            helpLocalizer = localizerFactory.Create(GetType());

            AdditionalArgumentsTitle = GetHelpBuilderLocalizedString(
                "DefaultHelpText.AdditionalArguments.Title",
                DefaultHelpText.AdditionalArguments.Title);
            AdditionalArgumentsDescription = GetHelpBuilderLocalizedString(
                "DefaultHelpText.AdditionalArguments.Description",
                DefaultHelpText.AdditionalArguments.Description);
            ArgumentsTitle = GetHelpBuilderLocalizedString(
                "DefaultHelpText.Arguments.Title",
                DefaultHelpText.Arguments.Title);
            CommandsTitle = GetHelpBuilderLocalizedString(
                "DefaultHelpText.Commands.Title",
                DefaultHelpText.Commands.Title);
            OptionsTitle = GetHelpBuilderLocalizedString(
                "DefaultHelpText.Options.Title",
                DefaultHelpText.Options.Title);
            UsageAdditionalArgumentsText = GetHelpBuilderLocalizedString(
                "DefaultHelpText.Usage.AdditionalArguments",
                DefaultHelpText.Usage.AdditionalArguments);
            UsageCommandText = GetHelpBuilderLocalizedString(
                "DefaultHelpText.Usage.Command",
                DefaultHelpText.Usage.Command);
            UsageOptionsText = GetHelpBuilderLocalizedString(
                "DefaultHelpText.Usage.Options",
                DefaultHelpText.Usage.Options);
            UsageTitle = GetHelpBuilderLocalizedString(
                "DefaultHelpText.Usage.Title",
                DefaultHelpText.Usage.Title);
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
            {
                lcmd.AddOption(GetLocalizedOption(option));
            }

            foreach (IArgument argument in command.Arguments)
            {
                lcmd.AddArgument(GetLocalizedArgument(argument));
            }

            lcmd.TreatUnmatchedTokensAsErrors = command.TreatUnmatchedTokensAsErrors;

            return lcmd;
        }

        private Option GetLocalizedOption(IOption option)
        {
            var lopt = new Option(option.RawAliases.First());
            Localize(lopt, option);

            lopt.Name = option.Name;
            if (!(option.Argument.Arity is { MaximumNumberOfValues: 0, MinimumNumberOfValues: 0 }))
            {
                lopt.Argument = GetLocalizedArgument(option.Argument);
            }

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
            {
                larg.SetDefaultValueFactory(() => argument.GetDefaultValue());
            }

            larg.AddSuggestions(txtToMatch => argument.GetSuggestions(txtToMatch)!);

            return larg;
        }

        private void Localize(Symbol symbol, ISymbol source)
        {
            if (!string.IsNullOrEmpty(source.Description))
            {
                var locDesc = localizer.GetString(source.Description);
                if (locDesc.ResourceNotFound)
                {
                    if (source.GetType().Name.Equals("HelpOption", StringComparison.Ordinal))
                    {
                        symbol.Description = GetHelpBuilderLocalizedString(
                            "HelpOption.Description",
                            source.Description ?? "");
                    }
                    else if (source.Name.Equals("version", StringComparison.OrdinalIgnoreCase))
                    {
                        symbol.Description = GetHelpBuilderLocalizedString(
                            "VersionOption.Description",
                            source.Description ?? "");
                    }
                }
                else
                {
                    symbol.Description = locDesc;
                }
            }

            foreach (var alias in source.RawAliases)
            {
                symbol.AddAlias(alias);
            }

            symbol.IsHidden = source.IsHidden;
        }

        protected override string DefaultValueHint(IArgument argument, bool isSingleArgument = true)
        {
            if (argument.HasDefaultValue && isSingleArgument && ShouldShowDefaultValueHint(argument))
            {
                var locDefault = helpLocalizer.GetString(
                    $"{nameof(HelpBuilder)}.{nameof(DefaultValueHint)}",
                    argument.GetDefaultValue());
                if (!(locDefault.ResourceNotFound || string.IsNullOrEmpty(locDefault)))
                {
                    return locDefault;
                }
            }

            return base.DefaultValueHint(argument, isSingleArgument);
        }

        private string GetHelpBuilderLocalizedString(string key, string @default)
        {
            var localized = helpLocalizer.GetString(key);
            string localizedValue = localized;
            if (string.IsNullOrEmpty(localizedValue) ||
                string.Equals(localizedValue, key, StringComparison.Ordinal))
            {
                return @default;
            }

            return localizedValue;
        }
    }
}
