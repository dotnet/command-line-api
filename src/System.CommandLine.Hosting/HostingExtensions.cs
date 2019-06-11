using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Linq;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace System.CommandLine.Hosting
{
    public static class HostingExtensions
    {
        public const string ConfigurationDirectiveName = "config";

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
                hostBuilder.UseConsoleLifetime();
                configureHost?.Invoke(hostBuilder);

                var invokeCancel = invocation.GetCancellationToken();

                using (var host = hostBuilder.Build())
                {
                    invocation.BindingContext.AddService(typeof(IHost), () => host);

                    await host.StartAsync(invokeCancel);

                    await next(invocation);

                    await host.StopAsync(invokeCancel);
                }
            });

        public static CommandLineBuilder UseHost(this CommandLineBuilder builder,
            Action<IHostBuilder> configureHost = null
            ) => UseHost(builder, null, configureHost);
    }
}
