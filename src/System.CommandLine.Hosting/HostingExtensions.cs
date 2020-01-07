using System.CommandLine.Binding;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Linq;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace System.CommandLine.Hosting
{
    public static class HostingExtensions
    {
        private const string ConfigurationDirectiveName = "config";

        public static CommandLineBuilder UseHost(this CommandLineBuilder builder,
            Func<string[], IHostBuilder> hostBuilderFactory,
            Action<IHostBuilder> configureHost = null) =>
            builder.UseMiddleware(async (invocation, next) =>
            {
                var argsRemaining = invocation.ParseResult.UnparsedTokens.ToArray();
                var hostBuilder = hostBuilderFactory?.Invoke(argsRemaining)
                    ?? new HostBuilder();
                hostBuilder.Properties[typeof(InvocationContext)] = invocation;

                hostBuilder.ConfigureHostConfiguration(config =>
                {
                    config.AddCommandLineDirectives(invocation.ParseResult, ConfigurationDirectiveName);
                });
                hostBuilder.ConfigureServices(services =>
                {
                    services.AddSingleton(invocation);
                    services.AddSingleton(invocation.BindingContext);
                    services.AddSingleton(invocation.Console);
                    services.AddTransient(_ => invocation.InvocationResult);
                    services.AddTransient(_ => invocation.ParseResult);
                });
                hostBuilder.UseInvocationLifetime(invocation);
                configureHost?.Invoke(hostBuilder);

                using (var host = hostBuilder.Build())
                {
                    invocation.BindingContext.AddService(typeof(IHost), () => host);

                    await host.StartAsync();

                    await next(invocation);

                    await host.StopAsync();
                }
            });

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
    }
}
