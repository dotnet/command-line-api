using System.CommandLine.Binding;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using Xunit;


namespace System.CommandLine.Hosting.Tests
{
    public static class HostingHandlerTest
    {
        [Fact]
        public static async Task Constructor_Injection_Injects_Service()
        {
            var service = new MyService();

            var parser = new CommandLineBuilder(
                new MyCommand { Handler = HostedCommandHandler.CreateFromHost<MyCommand.MyHandler>() }
                )
                .UseHost((builder) =>
                {
                    builder.ConfigureServices((context, services) =>
                    {
                        services.AddTransient<MyCommand.MyHandler>();
                        services.AddOptions<MyCommand.MyOptions>()
                            .BindCommandLine();
                        services.AddTransient(x => service);
                    });
                })
                .Build();

            var result = await parser.InvokeAsync(new string[] { "--int-option", "54" });

            service.Value.Should().Be(54);
            result.Should().Be(54);
        }

        [Fact]
        public static async Task Parameter_is_available_in_property()
        {
            var parser = new CommandLineBuilder(
                new MyCommand { Handler = HostedCommandHandler.CreateFromHost<MyCommand.MyHandler>() }
                )
                .UseHost(host =>
                {
                    host.ConfigureServices(services =>
                    {
                        services.AddTransient<MyCommand.MyHandler>();
                        services.AddOptions<MyCommand.MyOptions>()
                            .BindCommandLine();
                        services.AddTransient<MyService>();
                    });
                })
                .Build();

            var result = await parser.InvokeAsync(new string[] { "--int-option", "54" });

            result.Should().Be(54);
        }

        [Fact]
        public static async Task Can_have_diferent_handlers_based_on_command()
        {
            var root = new RootCommand();

            root.AddCommand(new MyCommand
            { 
                Handler = HostedCommandHandler.CreateFromHost<MyCommand.MyHandler>() 
            });
            root.AddCommand(new MyOtherCommand
            {
                Handler = HostedCommandHandler.CreateFromHost<MyOtherCommand.MyHandler>() 
            });
            var parser = new CommandLineBuilder(root)
                .UseHost(host =>
                {
                    host.ConfigureServices(services =>
                    {
                        services.AddTransient<MyCommand.MyHandler>();
                        services.AddOptions<MyCommand.MyOptions>()
                            .BindCommandLine();
                        services.AddTransient<MyOtherCommand.MyHandler>();
                        services.AddOptions<MyOtherCommand.MyOptions>()
                            .BindCommandLine();
                        services.AddTransient<MyService>(_ => new MyService()
                        {
                            Action = () => 100
                        });
                    });
                })
                .Build();

            var result = await parser.InvokeAsync(new string[] { "mycommand", "--int-option", "54" });

            result.Should().Be(54);

            result = await parser.InvokeAsync(new string[] { "myothercommand", "--int-option", "54" });

            result.Should().Be(100);
        }

        [Fact]
        public static async Task Can_bind_to_arguments_via_injection()
        {
            var service = new MyService();
            var cmd = new RootCommand();
            cmd.AddCommand(new MyOtherCommand
            {
                Handler = HostedCommandHandler.CreateFromHost<MyOtherCommand.MyHandler>() 
            });
            var parser = new CommandLineBuilder(cmd)
                .UseHost(host =>
                {
                    host.ConfigureServices(services =>
                    {
                        services.AddTransient<MyOtherCommand.MyHandler>();
                        services.AddOptions<MyOtherCommand.MyOptions>()
                            .BindCommandLine();
                        services.AddSingleton<MyService>(service);
                    });
                })
                .Build();

            var result = await parser.InvokeAsync(new string[] { "myothercommand", "TEST" });

            service.StringValue.Should().Be("TEST");
        }

        [Fact]
        public static void Throws_When_Injected_HandlerType_is_not_ICommandHandler()
        {
            new object().Invoking(_ =>
            {
                var handlerWrapper = HostedCommandHandler.CreateFromHost(
                    typeof(MyNonCommandHandler));
            }).Should().ThrowExactly<ArgumentException>(
                because: $"{typeof(MyNonCommandHandler)} does not implement {typeof(ICommandHandler)}"
            );
        }

        [Fact]
        public static void Throws_When_Injected_HandlerType_is_null()
        {
            new object().Invoking(_ =>
            {
                var handlerWrapper = HostedCommandHandler.CreateFromHost(null);
            }).Should().ThrowExactly<ArgumentNullException>();
        }

        public class MyCommand : Command
        {
            public MyCommand() : base(name: "mycommand")
            {
                AddOption(new Option<int>("--int-option")); // or nameof(Handler.IntOption).ToKebabCase() if you don't like the string literal
            }

            public class MyOptions
            {
                public int IntOption { get; set; } // bound from option
                public IConsole Console { get; set; } // bound from DI
            }

            public class MyHandler : ICommandHandler
            {
                private readonly MyService service;
                private readonly MyOptions options;

                public MyHandler(MyService service, IOptions<MyOptions> options)
                {
                    this.service = service;
                    this.options = options.Value;
                }

                public Task<int> InvokeAsync(InvocationContext context)
                {
                    service.Value = options.IntOption;
                    return Task.FromResult(options.IntOption);
                }
            }
        }

        public class MyOtherCommand : Command
        {
            public MyOtherCommand() : base(name: "myothercommand")
            {
                AddOption(new Option<int>("--int-option")); // or nameof(Handler.IntOption).ToKebabCase() if you don't like the string literal
                AddArgument(new Argument<string>("One"));
            }

            public class MyOptions
            {
                public int IntOption { get; set; } // bound from option
                public IConsole Console { get; set; } // bound from DI
                public string One { get; set; }
            }

            public class MyHandler : ICommandHandler
            {
                private readonly MyService service;
                private readonly MyOptions options;

                public MyHandler(MyService service, IOptions<MyOptions> options)
                {
                    this.service = service;
                    this.options = options.Value;
                }


                public Task<int> InvokeAsync(InvocationContext context)
                {
                    service.Value = options.IntOption;
                    service.StringValue = options.One;
                    return Task.FromResult(service.Action?.Invoke() ?? 0);
                }
            }
        }

        public class MyService
        {
            public Func<int> Action { get; set; }

            public int Value { get; set; }

            public string StringValue { get; set; }
        }

        public class MyNonCommandHandler
        {
            public static int DoSomething() => 0;
        }
    }
}
