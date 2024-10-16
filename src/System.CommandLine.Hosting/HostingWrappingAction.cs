#nullable enable

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.Threading;
using System.Threading.Tasks;

namespace System.CommandLine.Hosting;

internal sealed class HostingWrappingAction<THostBuilder> : HostingAction<THostBuilder>
{
    private readonly AsynchronousCliAction? _actualAction;

    internal static void SetHandlers(
        CliCommand command,
        Func<string[], THostBuilder> hostBuilderFactory,
        Action<THostBuilder>? configureHost = null,
        Func<THostBuilder, IHostBuilder>? builderAsHostBuilder = null)
    {
        if (command.Action is HostingAction<THostBuilder> hostingAction &&
            hostingAction.ConfigureHost != configureHost)
        {
            hostingAction.ConfigureHost =
                configureHost + hostingAction.ConfigureHost;
        }
        else if (command.Action is null || !HostingAction.IsHostingAction(command.Action))
        {
            command.Action = new HostingWrappingAction<THostBuilder>(
                command.Action as AsynchronousCliAction,
                hostBuilderFactory,
                configureHost,
                builderAsHostBuilder
                );
            command.TreatUnmatchedTokensAsErrors = false;
        }

        foreach (CliCommand subCommand in command.Subcommands)
        {
            SetHandlers(subCommand, hostBuilderFactory, configureHost, builderAsHostBuilder);
        }
    }

    internal HostingWrappingAction(
        AsynchronousCliAction? wrappedAction,
        Func<string[], THostBuilder> hostBuilderFactory,
        Action<THostBuilder>? configureHost = null,
        Func<THostBuilder, IHostBuilder>? builderAsHostBuilder = null
        ) : base(hostBuilderFactory, configureHost, builderAsHostBuilder)
    {
        _actualAction = wrappedAction;
    }

    protected override Task<int> InvokeHostAsync(IHost host, CancellationToken cancellationToken)
    {
        if (_actualAction is null) return Task.FromResult(0);
        var parseResult = host.Services.GetRequiredService<ParseResult>();
        return _actualAction?.InvokeAsync(parseResult, cancellationToken)
            ?? Task.FromResult(0);
    }

    public override BindingContext GetBindingContext(ParseResult parseResult)
        => _actualAction switch
        {
            BindingHandler bindingHandler => bindingHandler.GetBindingContext(parseResult),
            _ => base.GetBindingContext(parseResult),
        };
}