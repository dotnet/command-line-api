using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using FluentAssertions;
using Microsoft.Extensions.Localization;
using Xunit;

namespace System.CommandLine.Localization.Tests
{
    public class LocalizationExtensionsTests
    {
        [Fact]
        public void UseLocalization_registers_IStringLocalizerFactory_to_binding_context()
        {
            bool asserted = false;
            var command = new RootCommand()
            {
                Handler = CommandHandler.Create((IStringLocalizerFactory localizerFactory) =>
                {
                    localizerFactory.Should().NotBeNull();

                    asserted = true;
                }),
            };
            var parser = new CommandLineBuilder(command)
                .UseLocalization()
                .Build();

            parser.InvokeAsync("").ConfigureAwait(false).GetAwaiter().GetResult();

            asserted.Should().BeTrue();
        }
    }
}
