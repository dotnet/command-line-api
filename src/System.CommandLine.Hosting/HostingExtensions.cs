#nullable enable

using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace System.CommandLine.Hosting
{
    public static class HostingExtensions
    {
        public const string ConfigurationDirectiveName = "config";

        private static CliConfiguration UseHost<THostBuilder>(
            this CliConfiguration config,
            Func<string[], THostBuilder> hostBuilderFactory,
            Action<THostBuilder>? configureHost = null,
            Func<THostBuilder, IHostBuilder>? builderAsHostBuilder = null
            )
        {
            config.ProcessTerminationTimeout = null;
            if (config.RootCommand is CliRootCommand root)
            {
                root.Add(new CliDirective(ConfigurationDirectiveName));
            }

            HostingWrappingAction<THostBuilder>.SetHandlers(
                config.RootCommand,
                hostBuilderFactory,
                configureHost,
                builderAsHostBuilder
                );

            return config;
        }

        public static CliConfiguration UseHost(
            this CliConfiguration config,
            Func<string[], IHostBuilder>? hostBuilderFactory,
            Action<IHostBuilder>? configureHost = null
            ) => UseHost(
                config,
                hostBuilderFactory ?? Host.CreateDefaultBuilder,
                configureHost,
                builderAsHostBuilder: default
                );

        public static CliConfiguration UseHost(
            this CliConfiguration config,
            Action<IHostBuilder>? configureHost = null
            ) => UseHost(config, hostBuilderFactory: null, configureHost);

#if NET8_0_OR_GREATER
        public static CliConfiguration UseHost(
            this CliConfiguration config,
            Func<string[], HostApplicationBuilder>? hostBuilderFactory,
            Action<HostApplicationBuilder>? configureHost = null
            ) => UseHost(
                config,
                hostBuilderFactory ?? Host.CreateApplicationBuilder,
                configureHost,
                static builder => new HostApplicationBuilderAsIHostBuilder(builder)
                );

        public static CliConfiguration UseHost(
            this CliConfiguration config,
            Action<HostApplicationBuilder>? configureHost = null
        ) => UseHost(config, hostBuilderFactory: null, configureHost);
#endif

        public static OptionsBuilder<TOptions> BindCommandLine<TOptions>(
            this OptionsBuilder<TOptions> optionsBuilder)
            where TOptions : class
        {
            if (optionsBuilder is null)
                throw new ArgumentNullException(nameof(optionsBuilder));
            return optionsBuilder.Configure<IServiceProvider>((opts, serviceProvider) =>
            {
                var modelBinder = serviceProvider
                    .GetService<ModelBinder<TOptions>>()
                    ?? new ModelBinder<TOptions>();
                var bindingContext = serviceProvider.GetRequiredService<BindingContext>();
                modelBinder.UpdateInstance(opts, bindingContext);
            });
        }

        public static CliCommand UseHostedService<THostedService>(this CliCommand command)
            where THostedService : CliHostedService
        {
            command.Action = HostedServiceAction.Create<THostedService>();
            return command;
        }

        public static CliCommand UseCommandHandler<THandler>(this CliCommand command)
            where THandler : CliAction
        {
            command.Action = CommandHandler.Create(
                typeof(THandler)
                .GetMethod(nameof(AsynchronousCliAction.InvokeAsync))!
                );
            return command;
        }

        public static HostingAction<IHostBuilder> UseHost(
            this AsynchronousCliAction action,
            Func<string[], IHostBuilder>? hostBuilderFactory,
            Action<IHostBuilder>? configureHost = null
            )
        {
            _ = action ?? throw new ArgumentNullException(nameof(action));
            if (HostingAction.IsHostingAction(action))
                return action as HostingAction<IHostBuilder> 
                    ?? throw new InvalidOperationException(
                        "Specified CliAction instance is a hosting action, but its HostBuilder generic type argument is not equal to IHostBuilder."
                        );

            var wrappingAction = new HostingWrappingAction<IHostBuilder>(
                action,
                hostBuilderFactory ?? Host.CreateDefaultBuilder,
                configureHost
                );
            return wrappingAction;
        }

        public static HostingAction<IHostBuilder> UseHost(
            this AsynchronousCliAction action,
            Action<IHostBuilder>? configureHost = null
            ) => UseHost(action, hostBuilderFactory: default, configureHost);

#if NET8_0_OR_GREATER
        public static HostingAction<HostApplicationBuilder> UseHost(
            this AsynchronousCliAction action,
            Func<string[], HostApplicationBuilder>? hostBuilderFactory,
            Action<HostApplicationBuilder>? configureHost = null
            )
        {
            _ = action ?? throw new ArgumentNullException(nameof(action));
            if (HostingAction.IsHostingAction(action))
                return action as HostingAction<HostApplicationBuilder> 
                    ?? throw new InvalidOperationException(
                        "Specified CliAction instance is a hosting action, but its HostBuilder generic type argument is not equal to HostApplicationBuilder."
                        );

            var wrappingAction = new HostingWrappingAction<HostApplicationBuilder>(
                action,
                hostBuilderFactory ?? Host.CreateApplicationBuilder,
                configureHost,
                static builder => new HostApplicationBuilderAsIHostBuilder(builder)
                );
            return wrappingAction;
        }

        public static HostingAction<HostApplicationBuilder> UseHost(
            this AsynchronousCliAction action,
            Action<HostApplicationBuilder>? configureHost = null
            ) => UseHost(action, hostBuilderFactory: default, configureHost);
#endif

        public static ParseResult GetParseResult(this IHostBuilder hostBuilder)
        {
            _ = hostBuilder ?? throw new ArgumentNullException(nameof(hostBuilder));

            if (hostBuilder.Properties.TryGetValue(typeof(ParseResult), out var ctxObj) &&
                ctxObj is ParseResult parseResult)
                return parseResult;

            throw new InvalidOperationException("Host builder has no command-line parse result registered to it.");
        }

        public static ParseResult GetParseResult(this HostBuilderContext context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            if (context.Properties.TryGetValue(typeof(ParseResult), out var ctxObj) &&
                ctxObj is ParseResult parseResult)
                return parseResult;

            throw new InvalidOperationException("Host builder context has no command-line parse result registered to it.");
        }

        public static IHost? GetHost(this ParseResult parseResult)
        {
            _ = parseResult ?? throw new ArgumentNullException(paramName: nameof(parseResult));
            var hostModelBinder = new ModelBinder<IHost>();
            var bindingContext = parseResult.GetBindingContext();
            return (IHost?)hostModelBinder.CreateInstance(bindingContext);
        }
    }
}
