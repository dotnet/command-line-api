using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Linq;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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

        public static InvocationContext GetInvocationContext(this IHostBuilder hostBuilder) =>
            GetInvocationContext(hostBuilder?.Properties);

        public static InvocationContext GetInvocationContext(this HostBuilderContext context) =>
            GetInvocationContext(context?.Properties);

        private static InvocationContext GetInvocationContext(IDictionary<object, object> properties)
        {
            object ctxObj = null;
            if (properties?.TryGetValue(typeof(InvocationContext), out ctxObj) ?? false)
                return ctxObj as InvocationContext;
            return null;
        }

        public static void ConfigureFromCommandLine<TOptions>(
            this IServiceCollection services, HostBuilderContext context)
            where TOptions : class
        {
            var bindingContext = context.GetInvocationContext()?.BindingContext;
            services.Configure<TOptions>(opts =>
            {
                var modelBinder = new ModelBinder<TOptions>();
                modelBinder.UpdateInstance(opts, bindingContext);
            });
        }
    }
}
