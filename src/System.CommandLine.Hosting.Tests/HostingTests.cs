using System.CommandLine.Binding;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        public static void UseHost_UnparsedTokens_can_propagate_to_Host_Configuration()
        {
            const string testArgument = "test";
            const string testKey = "unparsed-config";
            string commandLineArgs = $"-- --{testKey} {testArgument}";

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
                    var args = invocation.ParseResult.UnparsedTokens.ToArray();
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
        public static void UseHost_UnparsedTokens_are_available_in_HostBuilder_factory()
        {
            const string testArgument = "test";
            const string testKey = "unparsed-config";
            string commandLineArgs = $"-- --{testKey} {testArgument}";

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
            rootCmd.AddOption(
                new Option($"-{nameof(MyOptions.MyArgument)}")
                { Argument = new Argument<int>() }
                );
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


        [Fact(Skip ="WIP")]
        public static async Task CommandLineHost_creates_host_for_simple_command()
        {
            //Arrange
            // var args = new string[] { $"--foo", "42" };
            MyOptions options = null;
            IHost hostToBind = null;

            var rootCmd = new RootCommand();
            rootCmd.AddOption(new Option($"--foo") { Argument = new Argument<int>() });
            rootCmd.Handler = CommandHandler.Create<IHost>((host) =>
            {
                hostToBind = host;
            });
            // Act
            // var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(1));
            CancellationTokenSource tokenSource = null;
            await CommandLineHost.CreateDefaultBuilder()
                .ConfigureCommandLineDefaults((CommandLineBuilder builder) =>
                {
                    // TODO: it is not possible to add it like this atm.
                    builder.AddCommand(rootCmd);
                })
                .Build()
                .RunAsync(tokenSource?.Token ?? default);
            // Assert
            Assert.NotNull(hostToBind);
            Assert.Equal(42, options.MyArgument);
        }

        [Fact]
        public static async Task CommandLineHost_contains_errors_in_ParseResult_service_for_not_mapped_input()
        {
            //Arrange
            // Act
            var host = CommandLineHost.CreateDefaultBuilder(new string[]{"--foo", "bar"})
                .ConfigureCommandLineDefaults((CommandLineBuilder builder) => {})
                .Build();
            var parseResult = host.Services.GetService<ParseResult>();
            await host.StartAsync();
            parseResult.Errors.Should().NotBeEmpty();
            //TODO: clarify how parsing errors and command execution exceptions should be handled
        }

        private class MyOptions
        {
            public int MyArgument { get; set; }
        }
    }
}
