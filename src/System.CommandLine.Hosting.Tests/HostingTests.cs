using System.CommandLine.Binding;
using System.Linq;
using CommandHandler = System.CommandLine.NamingConventionBinder.CommandHandler;

using FluentAssertions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using Xunit;
using System.Threading.Tasks;

namespace System.CommandLine.Hosting.Tests
{
    public static class HostingTests
    {
        [Fact]
        public async static Task UseHost_registers_IHost_to_binding_context()
        {
            IHost hostFromHandler = null;

            void Execute(IHost host)
            {
                hostFromHandler = host;
            }

            var config = new CliConfiguration(
                new CliRootCommand { Action = CommandHandler.Create<IHost>(Execute) }
                )
                .UseHost();

            await config.InvokeAsync(Array.Empty<string>());

            hostFromHandler.Should().NotBeNull();
        }

        [Fact]
        public async static Task UseHost_adds_ParseResult_to_HostBuilder_Properties()
        {
            ParseResult parseResult = null;

            var config = new CliConfiguration(new CliRootCommand())
                .UseHost(host =>
                {
                    if (host.Properties.TryGetValue(typeof(ParseResult), out var ctx))
                        parseResult = ctx as ParseResult;
                });

            await config.InvokeAsync(Array.Empty<string>());

            parseResult.Should().NotBeNull();
        }

        [Fact]
        public async static Task UseHost_adds_ParseResult_to_Host_Services()
        {
            BindingContext bindingContext = null;
            ParseResult parseResult = null;

            void Execute(IHost host)
            {
                var services = host.Services;
                bindingContext = services.GetRequiredService<BindingContext>();
                parseResult = services.GetRequiredService<ParseResult>();
            }

            var config = new CliConfiguration(
                new CliRootCommand { Action = CommandHandler.Create<IHost>(Execute) }
                )
                .UseHost();

            await config.InvokeAsync(Array.Empty<string>());

            bindingContext.Should().NotBeNull();
            parseResult.Should().NotBeNull();
        }

        [Fact]
        public static async Task UseHost_UnmatchedTokens_can_propagate_to_Host_Configuration()
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

            var config = new CliConfiguration(
                new CliRootCommand
                {
                    Action = CommandHandler.Create<IHost>(Execute),
                })
                .UseHost(host =>
                {
                    var parseResult = (ParseResult)host.Properties[typeof(ParseResult)];
                    var args = parseResult.UnmatchedTokens.ToArray();
                    host.ConfigureHostConfiguration(config =>
                    {
                        config.AddCommandLine(args);
                    });
                });

            await config.InvokeAsync(commandLineArgs);

            testConfigValue.Should().BeEquivalentTo(testArgument);
        }

        [Fact]
        public async static Task UseHost_UnmatchedTokens_are_available_in_HostBuilder_factory()
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

            var config = new CliConfiguration(
                new CliRootCommand
                {
                    Action = CommandHandler.Create<IHost>(Execute),
                })
                .UseHost(args =>
                {
                    var host = new HostBuilder();

                    host.ConfigureHostConfiguration(config =>
                    {
                        config.AddCommandLine(args);
                    });

                    return host;
                });

            await config.InvokeAsync(commandLineArgs);

            testConfigValue.Should().BeEquivalentTo(testArgument);
        }

        [Fact]
        public static async Task UseHost_flows_config_directives_to_HostConfiguration()
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

            var config = new CliConfiguration(
                new CliRootCommand
                {
                    Action = CommandHandler.Create<IHost>(Execute)
                })
                .UseHost();

            await config.InvokeAsync(commandLine);

            testConfigValue.Should().BeEquivalentTo(testValue);
        }

        [Fact]
        public static void UseHost_binds_parsed_arguments_to_options()
        {
            const int myValue = 4224;
            string commandLine = $"-{nameof(MyOptions.MyArgument)} {myValue}";
            MyOptions options = null;

            var rootCmd = new CliRootCommand();
            rootCmd.Options.Add(new CliOption<int>($"-{nameof(MyOptions.MyArgument)}"));
            rootCmd.Action = CommandHandler.Create((IHost host) =>
            {
                options = host.Services
                    .GetRequiredService<IOptions<MyOptions>>()
                    .Value;
            });

            int result = new CliConfiguration(rootCmd)
                .UseHost(host =>
                {
                    host.ConfigureServices(services =>
                    {
                        services.AddOptions<MyOptions>().BindCommandLine();
                    });
                })
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
        public async static Task GetParseResult_returns_non_null_instance()
        {
            bool ctxAsserted = false;
            var config = new CliConfiguration(new CliRootCommand())
                .UseHost(hostBuilder =>
                {
                    ParseResult ctx = hostBuilder.GetParseResult();
                    ctx.Should().NotBeNull();
                    ctxAsserted = true;
                });

            await config.InvokeAsync(string.Empty);
            ctxAsserted.Should().BeTrue();
        }

        [Fact]
        public async static Task GetParseResult_in_ConfigureServices_returns_non_null_instance()
        {
            bool ctxAsserted = false;
            var config = new CliConfiguration(new CliRootCommand())
                .UseHost(hostBuilder =>
                {
                    hostBuilder.ConfigureServices((hostingCtx, services) =>
                    {
                        ParseResult invocationCtx = hostingCtx.GetParseResult();
                        invocationCtx.Should().NotBeNull();
                        ctxAsserted = true;
                    });
                });

            await config.InvokeAsync(string.Empty);
            ctxAsserted.Should().BeTrue();
        }

        [Fact]
        public static void GetInvocationContext_throws_if_not_within_invocation()
        {
            var hostBuilder = new HostBuilder();
            hostBuilder.Invoking(b =>
            {
                _ = b.GetParseResult();
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
                    _ = hostingCtx.GetParseResult();
                });
                _ = b.Build();
            })
                .Should().Throw<InvalidOperationException>();
        }
    }
}
