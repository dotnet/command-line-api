using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests;

public class InvocationConfigurationTests
{
    [Fact]
    public void It_can_be_subclassed_to_provide_additional_context()
    {
        var command = new RootCommand();
        var commandWasInvoked = false;
        command.SetAction(parseResult =>
        {
            var appConfig = (CustomAppConfiguration)parseResult.InvocationConfiguration;

            // access custom config

            commandWasInvoked = true;

            return 0;
        });

        var config = new CustomAppConfiguration();

        command.Parse("").Invoke(config);

        commandWasInvoked.Should().BeTrue();
    }

    public class CustomAppConfiguration : InvocationConfiguration
    {
        public CustomAppConfiguration()
        {
            EnableDefaultExceptionHandler = false;
        }

        public IServiceProvider ServiceProvider { get; }
    }
}