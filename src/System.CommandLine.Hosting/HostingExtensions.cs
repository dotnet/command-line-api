using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.Linq;
using CommandHandler = System.CommandLine.NamingConventionBinder.CommandHandler;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace System.CommandLine.Hosting
{
    public static class HostingExtensions
    {
        public static CommandLineBuilder UseHost(this CommandLineBuilder builder,
            Func<string[], IHostBuilder> hostBuilderFactory,
            Action<IHostBuilder> configureHost = null)
        {
            Directive configurationDirective = new("config");
            builder.Directives.Add(configurationDirective);

            return builder.AddMiddleware(async (invocation, cancellationToken, next) =>
            {
                var argsRemaining = invocation.ParseResult.UnmatchedTokens.ToArray();
                var hostBuilder = hostBuilderFactory?.Invoke(argsRemaining)
                    ?? new HostBuilder();
                hostBuilder.Properties[typeof(InvocationContext)] = invocation;

                hostBuilder.ConfigureHostConfiguration(config =>
                {
                    config.AddCommandLineDirectives(invocation.ParseResult, configurationDirective);
                });
                var bindingContext = invocation.GetBindingContext();
                hostBuilder.ConfigureServices(services =>
                {
                    services.AddSingleton(invocation);
                    services.AddSingleton(bindingContext);
                    services.AddSingleton(invocation.Console);
                    services.AddTransient(_ => invocation.InvocationResult);
                    services.AddTransient(_ => invocation.ParseResult);
                });
                hostBuilder.UseInvocationLifetime(invocation);
                configureHost?.Invoke(hostBuilder);

                using var host = hostBuilder.Build();

                bindingContext.AddService(typeof(IHost), _ => host);

                await host.StartAsync(cancellationToken);

                await next(invocation, cancellationToken);

                await host.StopAsync(cancellationToken);
            });
        }

        public static CommandLineBuilder UseHost(this CommandLineBuilder builder,
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

        public static IHostBuilder UseCommandHandler<TCommand, THandler>(this IHostBuilder builder)
            where TCommand : Command
            where THandler : ICommandHandler
        {
            return builder.UseCommandHandler(typeof(TCommand), typeof(THandler));
        }

        public static IHostBuilder UseCommandHandler(this IHostBuilder builder, Type commandType, Type handlerType)
        {
            if (!typeof(Command).IsAssignableFrom(commandType))
            {
                throw new ArgumentException($"{nameof(commandType)} must be a type of {nameof(Command)}", nameof(handlerType));
            }

            if (!typeof(ICommandHandler).IsAssignableFrom(handlerType))
            {
                throw new ArgumentException($"{nameof(handlerType)} must implement {nameof(ICommandHandler)}", nameof(handlerType));
            }

            if (builder.Properties[typeof(InvocationContext)] is InvocationContext invocation 
                && invocation.ParseResult.CommandResult.Command is Command command
                && command.GetType() == commandType)
            {
                builder.ConfigureServices(services =>
                {
                    services.AddTransient(handlerType);
                });

                BindingHandler bindingHandler = CommandHandler.Create(handlerType.GetMethod(nameof(ICommandHandler.InvokeAsync)));
                command.Handler = bindingHandler;
                bindingHandler.GetBindingContext(invocation).AddService(handlerType, c => c.GetService<IHost>().Services.GetService(handlerType));
            }

            return builder;
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
    }
}
