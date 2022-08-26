using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Linq;
using CommandHandler = System.CommandLine.NamingConventionBinder.CommandHandler;

using FluentAssertions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using Xunit;

namespace System.CommandLine.Hosting.Tests
{
    public static class HostingTests
    {
        [Fact]
        public static void UseHost_registers_IHost_to_binding_context()
        {
            IHost hostFromHandler = null;

            void Execute(IHost host)
            {
                hostFromHandler = host;
            }

            var parser = new CommandLineBuilder(
                new RootCommand { Handler = CommandHandler.Create<IHost>(Execute) }
                )
                .UseHost()
                .Build();

            parser.InvokeAsync(Array.Empty<string>())
                .GetAwaiter().GetResult();

            hostFromHandler.Should().NotBeNull();
        }

        [Fact]
        public static void UseHost_adds_invocation_context_to_HostBuilder_Properties()
        {
            InvocationContext invocationContext = null;

            var parser = new CommandLineBuilder()
                .UseHost(host =>
                {
                    if (host.Properties.TryGetValue(typeof(InvocationContext), out var ctx))
                        invocationContext = ctx as InvocationContext;
                })
                .Build();

            parser.InvokeAsync(Array.Empty<string>())
                .GetAwaiter().GetResult();

            invocationContext.Should().NotBeNull();
        }

        [Fact]
        public static void UseHost_adds_invocation_context_to_Host_Services()
        {
            InvocationContext invocationContext = null;
            BindingContext bindingContext = null;
            ParseResult parseResult = null;
            IConsole console = null;

            void Execute(IHost host)
            {
                var services = host.Services;
                invocationContext = services.GetRequiredService<InvocationContext>();
                bindingContext = services.GetRequiredService<BindingContext>();
                parseResult = services.GetRequiredService<ParseResult>();
                console = services.GetRequiredService<IConsole>();
            }

            var parser = new CommandLineBuilder(
                new RootCommand { Handler = CommandHandler.Create<IHost>(Execute) }
                )
                .UseHost()
                .Build();

            parser.InvokeAsync(Array.Empty<string>())
                .GetAwaiter().GetResult();

            invocationContext.Should().NotBeNull();
            bindingContext.Should().NotBeNull();
            parseResult.Should().NotBeNull();
            console.Should().NotBeNull();
        }

        [Fact]
        public static void UseHost_UnmatchedTokens_can_propagate_to_Host_Configuration()
        {
            const string testArgument = "test";
            const string testKey = "unmatched-config";
            string commandLineArgs = $"--{testKey} {testArgument}";

            string testConfigValue = null;

            void Execute(IHost host)
            {
                var config = host.Services.GetRequiredService<IConfiguration>();
                testConfigValue = config[testKey];
            }

            var parser = new CommandLineBuilder(
                new RootCommand
                {
                    Handler = CommandHandler.Create<IHost>(Execute),
                })
                .UseHost(host =>
                {
                    var invocation = (InvocationContext)host.Properties[typeof(InvocationContext)];
                    var args = invocation.ParseResult.UnmatchedTokens.ToArray();
                    host.ConfigureHostConfiguration(config =>
                    {
                        config.AddCommandLine(args);
                    });
                })
                .Build();

            parser.InvokeAsync(commandLineArgs)
                .GetAwaiter().GetResult();

            testConfigValue.Should().BeEquivalentTo(testArgument);
        }

        [Fact]
        public static void UseHost_UnmatchedTokens_are_available_in_HostBuilder_factory()
        {
            const string testArgument = "test";
            const string testKey = "unmatched-config";
            string commandLineArgs = $"--{testKey} {testArgument}";

            string testConfigValue = null;

            void Execute(IHost host)
            {
                var config = host.Services.GetRequiredService<IConfiguration>();
                testConfigValue = config[testKey];
            }

            var parser = new CommandLineBuilder(
                new RootCommand
                {
                    Handler = CommandHandler.Create<IHost>(Execute),
                })
                .UseHost(args =>
                {
                    var host = new HostBuilder();

                    host.ConfigureHostConfiguration(config =>
                    {
                        config.AddCommandLine(args);
                    });

                    return host;
                })
                .Build();

            parser.InvokeAsync(commandLineArgs)
                .GetAwaiter().GetResult();

            testConfigValue.Should().BeEquivalentTo(testArgument);
        }

        [Fact]
        public static void UseHost_flows_config_directives_to_HostConfiguration()
        {
            const string testKey = "Test";
            const string testValue = "Value";
            string commandLine = $"[config:{testKey}={testValue}]";

            string testConfigValue = null;

            void Execute(IHost host)
            {
                var config = host.Services.GetRequiredService<IConfiguration>();
                testConfigValue = config[testKey];
            }

            var parser = new CommandLineBuilder(
                new RootCommand
                {
                    Handler = CommandHandler.Create<IHost>(Execute)
                })
                .UseHost()
                .Build();

            parser.InvokeAsync(commandLine).GetAwaiter().GetResult();

            testConfigValue.Should().BeEquivalentTo(testValue);
        }

        [Fact]
        public static void UseHost_binds_parsed_arguments_to_options()
        {
            const int myValue = 4224;
            string commandLine = $"-{nameof(MyOptions.MyArgument)} {myValue}";
            MyOptions options = null;

            var rootCmd = new RootCommand();
            rootCmd.AddOption(new Option<int>($"-{nameof(MyOptions.MyArgument)}"));
            rootCmd.Handler = CommandHandler.Create((IHost host) =>
            {
                options = host.Services
                    .GetRequiredService<IOptions<MyOptions>>()
                    .Value;
            });

            int result = new CommandLineBuilder(rootCmd)
                .UseHost(host =>
                {
                    host.ConfigureServices(services =>
                    {
                        services.AddOptions<MyOptions>().BindCommandLine();
                    });
                })
                .Build()
                .Invoke(commandLine);

            Assert.Equal(0, result);
            Assert.NotNull(options);
            Assert.Equal(myValue, options.MyArgument);
        }

        private class MyOptions
        {
            public int MyArgument { get; set; }
        }

        private class MyService
        {
            public int SomeValue { get; set; }
        }

        private class CommandExecuter
        {
            public CommandExecuter(MyService service)
            {
                Service = service;
            }

            public MyService Service { get; }

            public void Execute(int myArgument)
            {
                Service.SomeValue = myArgument;
            }

            public void SubCommand(int myArgument)
            {
                Service.SomeValue = myArgument;
            }
        }

        [Fact]
        public static void GetInvocationContext_returns_non_null_instance()
        {
            bool ctxAsserted = false;
            var parser = new CommandLineBuilder()
                .UseHost(hostBuilder =>
                {
                    InvocationContext ctx = hostBuilder.GetInvocationContext();
                    ctx.Should().NotBeNull();
                    ctxAsserted = true;
                })
                .Build();

            _ = parser.Invoke(string.Empty);
            ctxAsserted.Should().BeTrue();
        }

        [Fact]
        public static void GetInvocationContext_in_ConfigureServices_returns_non_null_instance()
        {
            bool ctxAsserted = false;
            var parser = new CommandLineBuilder()
                .UseHost(hostBuilder =>
                {
                    hostBuilder.ConfigureServices((hostingCtx, services) =>
                    {
                        InvocationContext invocationCtx = hostingCtx.GetInvocationContext();
                        invocationCtx.Should().NotBeNull();
                        ctxAsserted = true;
                    });
                })
                .Build();

            _ = parser.Invoke(string.Empty);
            ctxAsserted.Should().BeTrue();
        }

        [Fact]
        public static void GetInvocationContext_returns_same_instance_as_outer_middleware()
        {
            InvocationContext ctxCustom = null;
            InvocationContext ctxHosting = null;

            var parser = new CommandLineBuilder()
                .AddMiddleware((context, next) =>
                {
                    ctxCustom = context;
                    return next(context);
                })
                .UseHost(hostBuilder =>
                {
                    ctxHosting = hostBuilder.GetInvocationContext();
                })
                .Build();

            _ = parser.Invoke(string.Empty);

            ctxHosting.Should().BeSameAs(ctxCustom);
        }

        [Fact]
        public static void GetInvocationContext_in_ConfigureServices_returns_same_instance_as_outer_middleware()
        {
            InvocationContext ctxCustom = null;
            InvocationContext ctxConfigureServices = null;

            var parser = new CommandLineBuilder()
                .AddMiddleware((context, next) =>
                {
                    ctxCustom = context;
                    return next(context);
                })
                .UseHost(hostBuilder =>
                {
                    hostBuilder.ConfigureServices((hostingCtx, services) =>
                    {
                        ctxConfigureServices = hostingCtx.GetInvocationContext();
                    });
                })
                .Build();

            _ = parser.Invoke(string.Empty);

            ctxConfigureServices.Should().BeSameAs(ctxCustom);
        }

        [Fact]
        public static void GetInvocationContext_throws_if_not_within_invocation()
        {
            var hostBuilder = new HostBuilder();
            hostBuilder.Invoking(b =>
            {
                _ = b.GetInvocationContext();
            })
                .Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public static void GetInvocationContext_in_ConfigureServices_throws_if_not_within_invocation()
        {
            new HostBuilder().Invoking(b =>
            {
                b.ConfigureServices((hostingCtx, services) =>
                {
                    _ = hostingCtx.GetInvocationContext();
                });
                _ = b.Build();
            })
                .Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public static void GetHost_returns_non_null_instance_in_subsequent_middleware()
        {
            bool hostAsserted = false;
            var parser = new CommandLineBuilder()
                .UseHost()
                .AddMiddleware((invCtx, next) =>
                {
                    IHost host = invCtx.GetHost();
                    host.Should().NotBeNull();
                    hostAsserted = true;

                    return next(invCtx);
                })
                .Build();

            _ = parser.Invoke(string.Empty);

            hostAsserted.Should().BeTrue();
        }

        [Fact]
        public static void GetHost_returns_null_when_no_host_in_invocation()
        {
            bool hostAsserted = false;
            var parser = new CommandLineBuilder()
                .AddMiddleware((invCtx, next) =>
                {
                    IHost host = invCtx.GetHost();
                    host.Should().BeNull();
                    hostAsserted = true;

                    return next(invCtx);
                })
                .Build();

            _ = parser.Invoke(string.Empty);

            hostAsserted.Should().BeTrue();
        }
    }
}
