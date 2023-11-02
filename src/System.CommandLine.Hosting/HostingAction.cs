using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace System.CommandLine.Hosting
{
    // It's a wrapper, that configures the host, starts it and then runs the actual action.
    internal sealed class HostingAction : BindingHandler
    {
        internal const string HostingDirectiveName = "config";

        private readonly Func<string[], IHostBuilder> _hostBuilderFactory;
        private readonly Action<IHostBuilder> _configureHost;
        private readonly AsynchronousCliAction _actualAction;

        internal static void SetHandlers(CliCommand command, Func<string[], IHostBuilder> hostBuilderFactory, Action<IHostBuilder> configureHost)
        {
            command.Action = new HostingAction(hostBuilderFactory, configureHost, (AsynchronousCliAction)command.Action);
            command.TreatUnmatchedTokensAsErrors = false; // to pass unmatched Tokens to host builder factory

            foreach (CliCommand subCommand in command.Subcommands)
            {
                SetHandlers(subCommand, hostBuilderFactory, configureHost);
            }
        }

        private HostingAction(Func<string[], IHostBuilder> hostBuilderFactory, Action<IHostBuilder> configureHost, AsynchronousCliAction actualAction)
        {
            _hostBuilderFactory = hostBuilderFactory;
            _configureHost = configureHost;
            _actualAction = actualAction;
        }

        public override BindingContext GetBindingContext(ParseResult parseResult)
            => _actualAction is BindingHandler bindingHandler
                   ? bindingHandler.GetBindingContext(parseResult)
                   : base.GetBindingContext(parseResult);

        public override async Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
        {
            var argsRemaining = parseResult.UnmatchedTokens;
            var hostBuilder = _hostBuilderFactory?.Invoke(argsRemaining.ToArray())
                              ?? new HostBuilder();
            hostBuilder.Properties[typeof(ParseResult)] = parseResult;

            if (parseResult.Configuration.RootCommand is CliRootCommand root &&
                root.Directives.SingleOrDefault(d => d.Name == HostingDirectiveName) is { } directive)
            {
                if (parseResult.GetResult(directive) is { } directiveResult)
                {
                    hostBuilder.ConfigureHostConfiguration(config =>
                    {
                        var kvpSeparator = new[] { '=' };

                        config.AddInMemoryCollection(directiveResult.Values.Select(s =>
                        {
                            var parts = s.Split(kvpSeparator, count: 2);
                            var key = parts[0];
                            var value = parts.Length > 1 ? parts[1] : null;
                            return new KeyValuePair<string, string>(key, value);
                        }).ToList());
                    });
                }
            }

            var bindingContext = GetBindingContext(parseResult);
            int registeredBefore = 0;
            hostBuilder.UseInvocationLifetime();
            hostBuilder.ConfigureServices(services =>
            {
                services.AddSingleton(parseResult);
                services.AddSingleton(bindingContext);

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
                    return await _actualAction.InvokeAsync(parseResult, cancellationToken);
                }

                return 0;
            }
            finally
            {
                await host.StopAsync(cancellationToken);
            }
        }
    }
}