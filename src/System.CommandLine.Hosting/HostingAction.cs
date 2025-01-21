#nullable enable

using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using static System.CommandLine.Hosting.HostingExtensions;

namespace System.CommandLine.Hosting;

public static class HostingAction
{
    public static bool IsHostingAction(CliAction action)
    {
        _ = action ?? throw new ArgumentNullException(nameof(action));
        var bindingHandlerType = typeof(BindingHandler);
        var hostingActionType = typeof(HostingAction<>);
        var actionType = action.GetType();
        if (!bindingHandlerType.IsAssignableFrom(actionType)) return false;
        for (Type? testType = actionType; testType != null && testType != bindingHandlerType; testType = testType.BaseType)
        {
            if (!testType.IsGenericType) continue;
            var genericType = testType.GetGenericTypeDefinition();
            if (genericType == hostingActionType) return true;
        }
        return false;
    }
}

public abstract class HostingAction<THostBuilder> : BindingHandler
{
    private static readonly char[] equalsSeparator = new char[] { '=' };
    
    private readonly Func<string[], THostBuilder> _hostBuilderFactory;
    private readonly Func<THostBuilder, IHostBuilder>? _builderAsIHostBuilder;

    public Action<THostBuilder>? ConfigureHost { get; set; }

    public HostingAction(
        Func<string[], THostBuilder> hostBuilderFactory,
        Action<THostBuilder>? configureHost = null,
        Func<THostBuilder, IHostBuilder>? builderAsHostBuilder = null
        )
    {
        _hostBuilderFactory = hostBuilderFactory 
            ?? throw new ArgumentNullException(nameof(hostBuilderFactory));
        ConfigureHost = configureHost;
        _builderAsIHostBuilder = builderAsHostBuilder;
    }

    public override async Task<int> InvokeAsync(
        ParseResult parseResult,
        CancellationToken cancellationToken = default
        )
    {
        var argsRemaining = parseResult.UnmatchedTokens.ToArray();
        var actualHostBuilder = _hostBuilderFactory(argsRemaining);
        var hostBuilder = _builderAsIHostBuilder?.Invoke(actualHostBuilder);
        hostBuilder ??= (IHostBuilder)actualHostBuilder!;
        hostBuilder.Properties[typeof(ParseResult)] = parseResult;
        if (parseResult.Configuration.RootCommand is CliRootCommand root
            && root.Directives.SingleOrDefault(d => d.Name == ConfigurationDirectiveName) is { } directive
            && parseResult.GetResult(directive) is { } directiveResult)
        {
            hostBuilder.ConfigureHostConfiguration(config =>
            {
                config.AddInMemoryCollection(directiveResult.Values.Select(s =>
                {
                    var parts = s.Split(equalsSeparator, count: 2);
                    var key = parts[0];
                    var value = parts.Length > 1 ? parts[1] : null;
                    return new KeyValuePair<string, string?>(key, value);
                }).ToList());
            });
        }

        var bindingContext = GetBindingContext(parseResult);
        int registeredBefore = 0;

        hostBuilder.ConfigureServices(services =>
        {
            services.AddSingleton(parseResult);
            services.AddSingleton(bindingContext);

            registeredBefore = services.Count;
        });

        if (ConfigureHost is not null)
        {
            ConfigureHost.Invoke(actualHostBuilder);

            hostBuilder.ConfigureServices(services =>
            {
                // "_configureHost" just registered types that might be needed in BindingContext
                for (int i = registeredBefore; i < services.Count; i++)
                {
                    Type captured = services[i].ServiceType;
                    bindingContext.AddService(captured, c => c.GetService<IHost>()?.Services.GetService(captured)!);
                }
            });
        }

        using var host = hostBuilder.Build();
        bindingContext.AddService(_ => host);

        await host.StartAsync(cancellationToken);
        try
        {
            return await InvokeHostAsync(host, cancellationToken);
        }
        finally
        {
            await host.StopAsync(cancellationToken);
        }
    }

    protected abstract Task<int> InvokeHostAsync(IHost host, CancellationToken cancellationToken);
}
