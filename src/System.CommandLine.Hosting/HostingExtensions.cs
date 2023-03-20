using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using CommandHandler = System.CommandLine.NamingConventionBinder.CommandHandler;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.CommandLine.Help;

namespace System.CommandLine.Hosting
{
    public static class HostingExtensions
    {
        public static CommandLineConfiguration UseHost(this CommandLineConfiguration builder,
            Func<string[], IHostBuilder> hostBuilderFactory,
            Action<IHostBuilder> configureHost = null)
        {
            builder.Directives.Add(new Directive("config"));

            AddIfNotPresent(builder.RootCommand, new HelpOption());
            AddIfNotPresent(builder.RootCommand, new VersionOption());

            HostingAction.SetHandlers(builder.RootCommand, hostBuilderFactory, configureHost);

            return builder;
        }

        public static CommandLineConfiguration UseHost(this CommandLineConfiguration builder,
            Action<IHostBuilder> configureHost = null
            ) => UseHost(builder, null, configureHost);

        public static IHostBuilder UseInvocationLifetime(this IHostBuilder host,
            InvocationContext invocation, Action<InvocationLifetimeOptions> configureOptions = null)
        {
            return host.ConfigureServices(services =>
            {
                services.TryAddSingleton(invocation);
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

        public static Command UseCommandHandler<THandler>(this Command command)
            where THandler : CliAction
        {
            command.Action = CommandHandler.Create(typeof(THandler).GetMethod(nameof(CliAction.InvokeAsync)));

            return command;
        }

        public static InvocationContext GetInvocationContext(this IHostBuilder hostBuilder)
        {
            _ = hostBuilder ?? throw new ArgumentNullException(nameof(hostBuilder));

            if (hostBuilder.Properties.TryGetValue(typeof(InvocationContext), out var ctxObj) &&
                ctxObj is InvocationContext invocationContext)
                return invocationContext;

            throw new InvalidOperationException("Host builder has no Invocation Context registered to it.");
        }

        public static InvocationContext GetInvocationContext(this HostBuilderContext context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            if (context.Properties.TryGetValue(typeof(InvocationContext), out var ctxObj) &&
                ctxObj is InvocationContext invocationContext)
                return invocationContext;

            throw new InvalidOperationException("Host builder has no Invocation Context registered to it.");
        }

        public static IHost GetHost(this InvocationContext invocationContext)
        {
            _ = invocationContext ?? throw new ArgumentNullException(paramName: nameof(invocationContext));
            var hostModelBinder = new ModelBinder<IHost>();
            return (IHost)hostModelBinder.CreateInstance(invocationContext.GetBindingContext());
        }

        private static void AddIfNotPresent<T>(Command command, T option) where T : Option
        {
            for (int i = 0; i < command.Options.Count; i++)
            {
                if (command.Options[i] is T)
                {
                    return;
                }
            }

            command.Options.Add(option);
        }
    }
}
