using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.CommandLine.Invocation;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Binding;

namespace System.CommandLine.Hosting
{
    // It's a wrapper, that configures the host, starts it and then runs the actual action.
    internal sealed class HostingAction : BindingHandler
    {
        private readonly Func<string[], IHostBuilder> _hostBuilderFactory;
        private readonly Action<IHostBuilder> _configureHost;
        private readonly CliAction _actualAction;

        internal static void SetHandlers(Command command, Func<string[], IHostBuilder> hostBuilderFactory, Action<IHostBuilder> configureHost)
        {
            command.Action = new HostingAction(hostBuilderFactory, configureHost, command.Action);

            foreach (Command subCommand in command.Subcommands)
            {
                SetHandlers(subCommand, hostBuilderFactory, configureHost);
            }
        }

        private HostingAction(Func<string[], IHostBuilder> hostBuilderFactory, Action<IHostBuilder> configureHost, CliAction actualAction)
        {
            _hostBuilderFactory = hostBuilderFactory;
            _configureHost = configureHost;
            _actualAction = actualAction;
        }

        public override BindingContext GetBindingContext(InvocationContext invocationContext) 
            => _actualAction is BindingHandler bindingHandler
                ? bindingHandler.GetBindingContext(invocationContext)
                : base.GetBindingContext(invocationContext);

        public async override Task<int> InvokeAsync(InvocationContext invocation, CancellationToken cancellationToken = default)
        {
            var argsRemaining = invocation.ParseResult.UnmatchedTokens;
            var hostBuilder = _hostBuilderFactory?.Invoke(argsRemaining)
                ?? new HostBuilder();
            hostBuilder.Properties[typeof(InvocationContext)] = invocation;

            Directive configurationDirective = invocation.ParseResult.Configuration.Directives.Single(d => d.Name == "config");
            hostBuilder.ConfigureHostConfiguration(config =>
            {
                config.AddCommandLineDirectives(invocation.ParseResult, configurationDirective);
            });
            var bindingContext = GetBindingContext(invocation);
            int registeredBefore = 0;
            hostBuilder.UseInvocationLifetime(invocation);
            hostBuilder.ConfigureServices(services =>
            {
                services.AddSingleton(invocation);
                services.AddSingleton(bindingContext);
                services.AddSingleton(invocation.Console);
                services.AddTransient(_ => invocation.ParseResult);

                registeredBefore = services.Count;
            });

            if (_configureHost is not null)
            {
                _configureHost.Invoke(hostBuilder);

                hostBuilder.ConfigureServices(services =>
                {
                    // "_configureHost" just registered types that might be needed in BindingContext
                    for (int i = registeredBefore; i < services.Count; i++)
                    {
                        Type captured = services[i].ServiceType;
                        bindingContext.AddService(captured, c => c.GetService<IHost>().Services.GetService(captured));
                    }
                });
            }

            using var host = hostBuilder.Build();

            bindingContext.AddService(typeof(IHost), _ => host);

            await host.StartAsync(cancellationToken);

            try
            {
                if (_actualAction is not null)
                {
                    return await _actualAction.InvokeAsync(invocation, cancellationToken);
                }
                return 0;
            }
            finally
            {
                await host.StopAsync(cancellationToken);
            }
        }

        public override int Invoke(InvocationContext context) => InvokeAsync(context).GetAwaiter().GetResult();
    }
}
