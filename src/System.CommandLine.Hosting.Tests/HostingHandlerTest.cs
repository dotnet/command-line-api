using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;


namespace System.CommandLine.Hosting.Tests
{
    public static class HostingHandlerTest
    {
        [Fact]
        public static async Task Constructor_Injection_Injects_Service()
        {
            var service = new MyService();

            var config = new CliConfiguration(
                new MyCommand().UseCommandHandler<MyCommand.MyHandler>()
                )
                .UseHost((builder) => {
                    builder.ConfigureServices(services =>
                    {
                        services.AddTransient(x => service);
                    });
                });

            var result = await config.InvokeAsync(new string[] { "--int-option", "54"});

            service.Value.Should().Be(54);
        }

        [Fact]
        public static async Task Parameter_is_available_in_property()
        {
            var config = new CliConfiguration(new MyCommand().UseCommandHandler<MyCommand.MyHandler>())
                .UseHost(host =>
                {
                    host.ConfigureServices(services =>
                    {
                        services.AddTransient<MyService>();
                    });
                });

            var result = await config.InvokeAsync(new string[] { "--int-option", "54"});

            result.Should().Be(54);
        }

        [Fact]
        public static async Task Can_have_different_handlers_based_on_command()
        {
            var root = new CliRootCommand();

            root.Subcommands.Add(new MyCommand().UseCommandHandler<MyCommand.MyHandler>());
            root.Subcommands.Add(new MyOtherCommand().UseCommandHandler<MyOtherCommand.MyHandler>());
            var config = new CliConfiguration(root)
                .UseHost(host =>
                {
                    host.ConfigureServices(services =>
                    {
                        services.AddTransient<MyService>(_ => new MyService()
                        {
                            Action = () => 100
                        });
                    });
                });

            var result = await config.InvokeAsync(new string[] { "mycommand", "--int-option", "54" });

            result.Should().Be(54);

            result = await config.InvokeAsync(new string[] { "myothercommand", "--int-option", "54" });

            result.Should().Be(100);
        }

        [Fact]
        public static async Task Can_bind_to_arguments_via_injection()
        {
            var service = new MyService();
            var cmd = new CliRootCommand();
            cmd.Subcommands.Add(new MyOtherCommand().UseCommandHandler<MyOtherCommand.MyHandler>());
            var config = new CliConfiguration(cmd)
                .UseHost(host =>
                {
                    host.ConfigureServices(services =>
                    {
                        services.AddSingleton<MyService>(service);
                    });
                });

            var result = await config.InvokeAsync(new string[] { "myothercommand", "TEST" });

            service.StringValue.Should().Be("TEST");
        }

        [Fact]
        public static async Task Invokes_DerivedClass()
        {
            var service = new MyService();

            var cmd = new CliRootCommand();
            cmd.Subcommands.Add(new MyCommand().UseCommandHandler<MyCommand.MyDerivedHandler>());
            cmd.Subcommands.Add(new MyOtherCommand().UseCommandHandler<MyOtherCommand.MyDerivedHandler>());
            var config = new CliConfiguration(cmd)
                         .UseHost((builder) => {
                             builder.ConfigureServices(services =>
                             {
                                 services.AddTransient(x => service);
                             });
                         });

            await config.InvokeAsync(new string[] { "mycommand", "--int-option", "54" });
            service.Value.Should().Be(54);

            await config.InvokeAsync(new string[] { "myothercommand", "TEST" });
            service.StringValue.Should().Be("TEST");
        }

        public abstract class MyBaseHandler : CliAction
        {
            public int IntOption { get; set; } // bound from option

            public override int Invoke(ParseResult context)
            {
                return Act();
            }

            public override Task<int> InvokeAsync(ParseResult context, CancellationToken cancellationToken)
            {
                return Task.FromResult(Act());
            }

            protected abstract int Act();
        }

        public class MyCommand : CliCommand
        {
            public MyCommand() : base(name: "mycommand")
            {
                Options.Add(new CliOption<int>("--int-option")); // or nameof(Handler.IntOption).ToKebabCase() if you don't like the string literal
            }

            public class MyHandler : CliAction
            {
                private readonly MyService service;

                public MyHandler(MyService service)
                {
                    this.service = service;
                }

                public int IntOption { get; set; } // bound from option

                public override int Invoke(ParseResult context)
                {
                    service.Value = IntOption;
                    return IntOption;
                }

                public override Task<int> InvokeAsync(ParseResult context, CancellationToken cancellationToken)
                {
                    service.Value = IntOption;
                    return Task.FromResult(IntOption);
                }
            }

            public class MyDerivedHandler : MyBaseHandler
            {
                private readonly MyService service;

                public MyDerivedHandler(MyService service)
                {
                    this.service = service;
                }

                protected override int Act()
                {
                    service.Value = IntOption;
                    return IntOption;
                }
            }
        }

        public class MyOtherCommand : CliCommand
        {
            public MyOtherCommand() : base(name: "myothercommand")
            {
                Options.Add(new CliOption<int>("--int-option")); // or nameof(Handler.IntOption).ToKebabCase() if you don't like the string literal
                Arguments.Add(new CliArgument<string>("One") {  Arity = ArgumentArity.ZeroOrOne });
            }

            public class MyHandler : CliAction
            {
                private readonly MyService service;

                public MyHandler(MyService service)
                {
                    this.service = service;
                }

                public int IntOption { get; set; } // bound from option

                public string One { get; set; }

                public override int Invoke(ParseResult context) => InvokeAsync(context, CancellationToken.None).GetAwaiter().GetResult();

                public override Task<int> InvokeAsync(ParseResult context, CancellationToken cancellationToken)
                {
                    service.Value = IntOption;
                    service.StringValue = One;
                    return Task.FromResult(service.Action?.Invoke() ?? 0);
                }
            }

            public class MyDerivedHandler : MyBaseHandler
            {
                private readonly MyService service;

                public MyDerivedHandler(MyService service)
                {
                    this.service = service;
                }

                public string One { get; set; }

                protected override int Act()
                {
                    service.Value = IntOption;
                    service.StringValue = One;
                    return service.Action?.Invoke() ?? 0;
                }
            }
        }

        public class MyService
        {
            public Func<int> Action { get; set; }

            public int Value { get; set; }

            public string StringValue { get; set; }
        }
    }
}
