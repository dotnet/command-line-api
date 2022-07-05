using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
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

            var parser = new CommandLineBuilder(
                new MyCommand()
                )
                .UseHost((builder) => {
                    builder.ConfigureServices(services =>
                    {
                        services.AddTransient(x => service);
                    })
                    .UseCommandHandler<MyCommand, MyCommand.MyHandler>();
                })
                .Build();

            var result = await parser.InvokeAsync(new string[] { "--int-option", "54"});

            service.Value.Should().Be(54);
        }

        [Fact]
        public static async Task Parameter_is_available_in_property()
        {
            var parser = new CommandLineBuilder(new MyCommand())
                .UseHost(host =>
                {
                    host.ConfigureServices(services =>
                    {
                        services.AddTransient<MyService>();
                    })
                    .UseCommandHandler<MyCommand, MyCommand.MyHandler>();
                })
                .Build();

            var result = await parser.InvokeAsync(new string[] { "--int-option", "54"});

            result.Should().Be(54);
        }

        [Fact]
        public static async Task Can_have_diferent_handlers_based_on_command()
        {
            var root = new RootCommand();

            root.AddCommand(new MyCommand());
            root.AddCommand(new MyOtherCommand());
            var parser = new CommandLineBuilder(root)
                .UseHost(host =>
                {
                    host.ConfigureServices(services =>
                    {
                        services.AddTransient<MyService>(_ => new MyService()
                        {
                            Action = () => 100
                        });
                    })
                    .UseCommandHandler<MyCommand, MyCommand.MyHandler>()
                    .UseCommandHandler<MyOtherCommand, MyOtherCommand.MyHandler>();
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
            cmd.AddCommand(new MyOtherCommand());
            var parser = new CommandLineBuilder(cmd)
                .UseHost(host =>
                {
                    host.ConfigureServices(services =>
                    {
                        services.AddSingleton<MyService>(service);
                    })
                    .UseCommandHandler<MyOtherCommand, MyOtherCommand.MyHandler>();
                })
                .Build();

            var result = await parser.InvokeAsync(new string[] { "myothercommand", "TEST" });

            service.StringValue.Should().Be("TEST");
        }

        [Fact]
        public static async Task Invokes_DerivedClass()
        {
            var service = new MyService();

            var cmd = new RootCommand();
            cmd.AddCommand(new MyCommand());
            cmd.AddCommand(new MyOtherCommand());
            var parser = new CommandLineBuilder(cmd)
                         .UseHost((builder) => {
                             builder.ConfigureServices(services =>
                             {
                                 services.AddTransient(x => service);
                             })
                                    .UseCommandHandler<MyCommand, MyCommand.MyDerivedHandler>()
                                    .UseCommandHandler<MyOtherCommand, MyOtherCommand.MyDerivedHandler>();
                         })
                         .Build();

            await parser.InvokeAsync(new string[] { "mycommand", "--int-option", "54" });
            service.Value.Should().Be(54);

            await parser.InvokeAsync(new string[] { "myothercommand", "TEST" });
            service.StringValue.Should().Be("TEST");
        }

        public abstract class MyBaseHandler : ICommandHandler
        {
            public int IntOption { get; set; } // bound from option
            public IConsole Console { get; set; } // bound from DI

            public int Invoke(InvocationContext context)
            {
                return Act();
            }

            public Task<int> InvokeAsync(InvocationContext context)
            {
                return Task.FromResult(Act());
            }

            protected abstract int Act();
        }

        public class MyCommand : Command
        {
            public MyCommand() : base(name: "mycommand")
            {
                AddOption(new Option<int>("--int-option")); // or nameof(Handler.IntOption).ToKebabCase() if you don't like the string literal
            }

            public class MyHandler : ICommandHandler
            {
                private readonly MyService service;

                public MyHandler(MyService service)
                {
                    this.service = service;
                }

                public int IntOption { get; set; } // bound from option
                public IConsole Console { get; set; } // bound from DI

                public int Invoke(InvocationContext context)
                {
                    service.Value = IntOption;
                    return IntOption;
                }

                public Task<int> InvokeAsync(InvocationContext context)
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

        public class MyOtherCommand : Command
        {
            public MyOtherCommand() : base(name: "myothercommand")
            {
                AddOption(new Option<int>("--int-option")); // or nameof(Handler.IntOption).ToKebabCase() if you don't like the string literal
                AddArgument(new Argument<string>("One"));
            }

            public class MyHandler : ICommandHandler
            {
                private readonly MyService service;

                public MyHandler(MyService service)
                {
                    this.service = service;
                }

                public int IntOption { get; set; } // bound from option
                public IConsole Console { get; set; } // bound from DI

                public string One { get; set; }

                public int Invoke(InvocationContext context) => InvokeAsync(context).GetAwaiter().GetResult();

                public Task<int> InvokeAsync(InvocationContext context)
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
