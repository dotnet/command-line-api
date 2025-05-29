using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using System.CommandLine.Parsing;

namespace System.CommandLine.Hosting
{
    public static class HostingExtensions
    {
        public static ParseResult GetParseResult(this IHostBuilder hostBuilder)
        {
            _ = hostBuilder ?? throw new ArgumentNullException(nameof(hostBuilder));

            if (hostBuilder.Properties.TryGetValue(typeof(ParseResult), out var ctxObj) &&
                ctxObj is ParseResult invocationContext)
                return invocationContext;

            throw new InvalidOperationException("Host builder has no command-line parse result registered to it.");
        }

        public static ParseResult GetParseResult(this HostBuilderContext context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            if (context.Properties.TryGetValue(typeof(ParseResult), out var ctxObj) &&
                ctxObj is ParseResult invocationContext)
                return invocationContext;

            throw new InvalidOperationException("Host builder context has no command-line parse result registered to it.");
        }

        internal static HostingAction GetHostingAction(this ParseResult parseResult)
        {
            if (!parseResult.TryGetHostingAction(out var hostingAction))
                throw new InvalidOperationException("Command-line parse result is not for a command with an associated .NET Generic Host command-line action.");
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
            return hostingAction;
#else
            return hostingAction!;
#endif
        }

        internal static bool TryGetHostingAction(
            this ParseResult parseResult,
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
            [Diagnostics.CodeAnalysis.NotNullWhen(returnValue: true)]
#endif
            out HostingAction? hostingAction
            )
        {
            hostingAction = parseResult.CommandResult.Command.Action
                as HostingAction;
            return hostingAction is not null;
        }

        private static void ConfigureHostBuilderOnSymbolValidation(
            this Symbol symbol,
            Action<
                IHostBuilder,
                SymbolResult
                > configureHostBuilderAction
            )
        {
            switch (symbol)
            {
                case Argument argument:
                    argument.Validators.Add(SymbolResultAction);
                    break;
                case Option option:
                    option.Validators.Add(SymbolResultAction);
                    break;
                case Command command:
                    command.Validators.Add(SymbolResultAction);
                    break;
                default:
                    throw new InvalidOperationException();
            }

            void SymbolResultAction(SymbolResult symbolResult)
            {
                CommandResult? commandResult = null;
                // Find nearest parent symbol result starting from current,
                // stop when a CommandResult is found
                for (SymbolResult? parentResult = symbolResult;
                    parentResult is not null &&
                    (commandResult = parentResult as CommandResult) is null;
                    parentResult = parentResult.Parent)
                { }

                // No CommandResult was found, strange but nothing to do here.
                if (commandResult is null) return;

                // CommandResult was found, but Command action is not a
                // .NET Generic Host action, so nothing to do in that case
                if (commandResult.Command.Action is not HostingAction hostingAction)
                { return; }

                // .NET Generic Host action identitfied,
                // register Model Binder configuration action
                hostingAction.ConfigureHost += hostBuilder =>
                    configureHostBuilderAction(hostBuilder, symbolResult);
            }
        }

        private static void ConfigureSymbolOptionsServices<TOptions, TValue>(
            this Symbol symbol,
            Action<TOptions, TValue?> optionsInstanceApplyValueAction,
            Action<
                HostBuilderContext,
                IServiceCollection,
                Action<TOptions>
                > configureServicesAction
            ) where TOptions : class
        {
            symbol.ConfigureHostBuilderOnSymbolValidation(ConfigureHostBuilder);

            void ConfigureHostBuilder(
                IHostBuilder hostBuilder,
                SymbolResult symbolResult
                )
            {
                hostBuilder.ConfigureServices(
                    (context, services) =>
                    ConfigureServices(context, services, symbolResult)
                    );
            }

            void ConfigureServices(
                HostBuilderContext context,
                IServiceCollection services,
                SymbolResult symbolResult
                )
            {
                configureServicesAction(context, services, options =>
                {
                    TValue? symbolValue = symbolResult switch
                    {
                        ArgumentResult { Argument: Argument<TValue> argument } =>
                            symbolResult.GetValue(argument),
                        OptionResult { Option: Option<TValue> option } =>
                            symbolResult.GetValue(option),
                        _ => throw new InvalidOperationException()
                    };
                    optionsInstanceApplyValueAction(options, symbolValue);
                });
            }
        }

        public static Option<TValue> ConfigureOptionsInstance<TOptions, TValue>(
            this Option<TValue> option,
            string? optionsName,
            Action<TOptions, TValue?> optionsInstanceApplyValueAction
            ) where TOptions : class
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(option);
#else
            _ = option ?? throw new ArgumentNullException(nameof(option));
#endif
            option.ConfigureSymbolOptionsServices(
                optionsInstanceApplyValueAction,
                (_, services, configureAction) => services
                .Configure(optionsName, configureAction)
                );
            return option;
        }

        public static Argument<TValue> ConfigureOptionsInstance<TOptions, TValue>(
            this Argument<TValue> argument,
            string? optionsName,
            Action<TOptions, TValue?> optionsInstanceApplyValueAction
            ) where TOptions : class
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(argument);
#else
            _ = argument ?? throw new ArgumentNullException(nameof(argument));
#endif
            argument.ConfigureSymbolOptionsServices(
                optionsInstanceApplyValueAction,
                (_, services, configureAction) => services
                .Configure(optionsName, configureAction)
                );
            return argument;
        }

        public static Option<TValue> ConfigureOptionsInstance<TOptions, TValue>(
            this Option<TValue> option,
            Action<TOptions, TValue?> optionsInstanceApplyValueAction
            ) where TOptions : class
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(option);
#else
            _ = option ?? throw new ArgumentNullException(nameof(option));
#endif
            option.ConfigureSymbolOptionsServices(
                optionsInstanceApplyValueAction,
                (_, services, configureAction) => services
                .Configure(configureAction)
                );
            return option;
        }

        public static Argument<TValue> ConfigureOptionsInstance<TOptions, TValue>(
            this Argument<TValue> argument,
            Action<TOptions, TValue?> optionsInstanceApplyValueAction
            ) where TOptions : class
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(argument);
#else
            _ = argument ?? throw new ArgumentNullException(nameof(argument));
#endif
            argument.ConfigureSymbolOptionsServices(
                optionsInstanceApplyValueAction,
                (_, services, configureAction) => services
                .Configure(configureAction)
                );
            return argument;
        }

        public static Option<TValue> ConfigureAllOptionsInstances<TOptions, TValue>(
            this Option<TValue> option,
            Action<TOptions, TValue?> optionsInstanceApplyValueAction
            ) where TOptions : class
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(option);
#else
            _ = option ?? throw new ArgumentNullException(nameof(option));
#endif
            option.ConfigureSymbolOptionsServices(
                optionsInstanceApplyValueAction,
                (_, services, configureAction) => services
                .ConfigureAll(configureAction)
                );
            return option;
        }

        public static Argument<TValue> ConfigureAllOptionsInstances<TOptions, TValue>(
            this Argument<TValue> argument,
            Action<TOptions, TValue?> optionsInstanceApplyValueAction
            ) where TOptions : class
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(argument);
#else
            _ = argument ?? throw new ArgumentNullException(nameof(argument));
#endif
            argument.ConfigureSymbolOptionsServices(
                optionsInstanceApplyValueAction,
                (_, services, configureAction) => services
                .ConfigureAll(configureAction)
                );
            return argument;
        }

        public static Option<TValue> PostConfigureOptionsInstance<TOptions, TValue>(
            this Option<TValue> option,
            string? optionsName,
            Action<TOptions, TValue?> optionsInstanceApplyValueAction
            ) where TOptions : class
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(option);
#else
            _ = option ?? throw new ArgumentNullException(nameof(option));
#endif
            option.ConfigureSymbolOptionsServices(
                optionsInstanceApplyValueAction,
                (_, services, configureAction) => services
                .PostConfigure(optionsName, configureAction)
                );
            return option;
        }

        public static Argument<TValue> PostConfigureOptionsInstance<TOptions, TValue>(
            this Argument<TValue> argument,
            string? optionsName,
            Action<TOptions, TValue?> optionsInstanceApplyValueAction
            ) where TOptions : class
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(argument);
#else
            _ = argument ?? throw new ArgumentNullException(nameof(argument));
#endif
            argument.ConfigureSymbolOptionsServices(
                optionsInstanceApplyValueAction,
                (_, services, configureAction) => services
                .PostConfigure(optionsName, configureAction)
                );
            return argument;
        }

        public static Option<TValue> PostConfigureOptionsInstance<TOptions, TValue>(
            this Option<TValue> option,
            Action<TOptions, TValue?> optionsInstanceApplyValueAction
            ) where TOptions : class
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(option);
#else
            _ = option ?? throw new ArgumentNullException(nameof(option));
#endif
            option.ConfigureSymbolOptionsServices(
                optionsInstanceApplyValueAction,
                (_, services, configureAction) => services
                .PostConfigure(configureAction)
                );
            return option;
        }

        public static Argument<TValue> PostConfigureOptionsInstance<TOptions, TValue>(
            this Argument<TValue> argument,
            Action<TOptions, TValue?> optionsInstanceApplyValueAction
            ) where TOptions : class
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(argument);
#else
            _ = argument ?? throw new ArgumentNullException(nameof(argument));
#endif
            argument.ConfigureSymbolOptionsServices(
                optionsInstanceApplyValueAction,
                (_, services, configureAction) => services
                .PostConfigure(configureAction)
                );
            return argument;
        }

        public static Option<TValue> PostConfigureAllOptionsInstances<TOptions, TValue>(
            this Option<TValue> option,
            Action<TOptions, TValue?> optionsInstanceApplyValueAction
            ) where TOptions : class
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(option);
#else
            _ = option ?? throw new ArgumentNullException(nameof(option));
#endif
            option.ConfigureSymbolOptionsServices(
                optionsInstanceApplyValueAction,
                (_, services, configureAction) => services
                .PostConfigureAll(configureAction)
                );
            return option;
        }

        public static Argument<TValue> PostConfigureAllOptionsInstances<TOptions, TValue>(
            this Argument<TValue> argument,
            Action<TOptions, TValue?> optionsInstanceApplyValueAction
            ) where TOptions : class
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(argument);
#else
            _ = argument ?? throw new ArgumentNullException(nameof(argument));
#endif
            argument.ConfigureSymbolOptionsServices(
                optionsInstanceApplyValueAction,
                (_, services, configureAction) => services
                .PostConfigureAll(configureAction)
                );
            return argument;
        }

        public static Command ConfigureHostBuilder(
            this Command command,
            Action<IHostBuilder, CommandResult> hostBuilderAction
            )
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(command);
#else
            _ = command ?? throw new ArgumentNullException(nameof(command));
#endif
            command.ConfigureHostBuilderOnSymbolValidation(
                (hostBuilder, symbolResult) => hostBuilderAction?.Invoke(
                    hostBuilder,
                    (CommandResult)symbolResult
                )
            );
            return command;
        }

        public static RootCommand ConfigureHostBuilder(
            this RootCommand rootCommand,
            Action<IHostBuilder, CommandResult> hostBuilderAction
            )
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(rootCommand);
#else
            _ = rootCommand ?? throw new ArgumentNullException(nameof(rootCommand));
#endif
            rootCommand.ConfigureHostBuilderOnSymbolValidation(
                (hostBuilder, symbolResult) => hostBuilderAction?.Invoke(
                    hostBuilder,
                    (CommandResult)symbolResult
                )
            );
            return rootCommand;
        }
    }
}
