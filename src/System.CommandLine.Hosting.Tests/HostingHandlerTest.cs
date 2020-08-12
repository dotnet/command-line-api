using System.CommandLine.Binding;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Xunit;


namespace System.CommandLine.Hosting.Tests
{
    public static class HostingHandlerTest
    {

        [Fact]
        public static void Constructor_Injection_Injects_Service()
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

            var result = parser.InvokeAsync(new string[] { "--int-option", "54"})
                .GetAwaiter().GetResult();

            service.Value.Should().Be(54);
        }

        [Fact]
        public static void Parameter_is_available_in_property()
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

            var result = parser.InvokeAsync(new string[] { "--int-option", "54"})
                .GetAwaiter().GetResult();

            result.Should().Be(54);
        }

        [Fact]
        public static void Can_have_diferent_handlers_based_on_command()
        {
            var root = new RootCommand();

            root.AddCommand(new MyCommand());
            root.AddCommand(new MyOtherCommand());
            var parser = new CommandLineBuilder(root)
                .UseHost(host =>
                {
                    host.ConfigureServices(services =>
                    {
                        services.AddTransient<MyService>();
                    })
                    .UseCommandHandler<MyCommand, MyCommand.MyHandler>()
                    .UseCommandHandler<MyOtherCommand, MyOtherCommand.MyHandler>();
                })
                .Build();

            var result = parser.InvokeAsync(new string[] { "mycommand", "--int-option", "54" })
                .GetAwaiter().GetResult();

            result.Should().Be(54);

            result = parser.InvokeAsync(new string[] { "myothercommand", "--int-option", "54" })
                .GetAwaiter().GetResult();

            result.Should().Be(540);
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

                public Task<int> InvokeAsync(InvocationContext context)
                {
                    service.Value = IntOption;
                    return Task.FromResult(IntOption);
                }
            }
        }

        public class MyOtherCommand : Command
        {
            public MyOtherCommand() : base(name: "myothercommand")
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

                public Task<int> InvokeAsync(InvocationContext context)
                {
                    service.Value = IntOption * 10;
                    return Task.FromResult(IntOption * 10);
                }
            }
        }

        public class MyService
        {
            public int Value { get; set; }
        }
    }
}
