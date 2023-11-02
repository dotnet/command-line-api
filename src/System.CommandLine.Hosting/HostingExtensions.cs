using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using CommandHandler = System.CommandLine.NamingConventionBinder.CommandHandler;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace System.CommandLine.Hosting
{
    public static class HostingExtensions
    {
        public static CliConfiguration UseHost(
            this CliConfiguration config,
            Func<string[], IHostBuilder> hostBuilderFactory,
            Action<IHostBuilder> configureHost = null)
        {
            if (config.RootCommand is CliRootCommand root)
            {
                root.Add(new CliDirective(HostingAction.HostingDirectiveName));
            }

            HostingAction.SetHandlers(config.RootCommand, hostBuilderFactory, configureHost);

            return config;
        }

        public static CliConfiguration UseHost(
            this CliConfiguration config,
            Action<IHostBuilder> configureHost = null
        ) => UseHost(config, null, configureHost);

        public static IHostBuilder UseInvocationLifetime(this IHostBuilder host, Action<InvocationLifetimeOptions> configureOptions = null)
        {
            return host.ConfigureServices(services =>
            {
                services.AddSingleton<IHostLifetime, InvocationLifetime>();
                if (configureOptions is Action<InvocationLifetimeOptions>)
                    services.Configure(configureOptions);
            });
        }

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

        public static CliCommand UseCommandHandler<THandler>(this CliCommand command)
            where THandler : CliAction
        {
            command.Action = CommandHandler.Create(typeof(THandler).GetMethod(nameof(AsynchronousCliAction.InvokeAsync)));

            return command;
        }

        public static ParseResult GetParseResult(this IHostBuilder hostBuilder)
        {
            _ = hostBuilder ?? throw new ArgumentNullException(nameof(hostBuilder));

            if (hostBuilder.Properties.TryGetValue(typeof(ParseResult), out var ctxObj) &&
                ctxObj is ParseResult invocationContext)
                return invocationContext;

            throw new InvalidOperationException("Host builder has no Invocation Context registered to it.");
        }

        public static ParseResult GetParseResult(this HostBuilderContext context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            if (context.Properties.TryGetValue(typeof(ParseResult), out var ctxObj) &&
                ctxObj is ParseResult invocationContext)
                return invocationContext;

            throw new InvalidOperationException("Host builder has no Invocation Context registered to it.");
        }

        public static IHost GetHost(this ParseResult parseResult)
        {
            _ = parseResult ?? throw new ArgumentNullException(paramName: nameof(parseResult));
            var hostModelBinder = new ModelBinder<IHost>();
            return (IHost)hostModelBinder.CreateInstance(parseResult.GetBindingContext());
        }
    }
}
