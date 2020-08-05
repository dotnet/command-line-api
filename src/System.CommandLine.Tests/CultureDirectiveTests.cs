using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Globalization;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    public class CultureDirectiveTests
    {
        [Fact]
        public static async Task Sets_CurrentCulture_to_directive_culture()
        {
            bool asserted = false;
            // Make sure we're invariant to begin with
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            var rootCommand = new RootCommand
            {
                Handler = CommandHandler.Create(() =>
                {
                    asserted = true;
                    CultureInfo.CurrentCulture.Name.Should().Be("de-DE");
                })
            };
            var parser = new CommandLineBuilder(rootCommand)
                .UseCultureDirective()
                .Build();

            await parser.InvokeAsync(new[] { "[culture:de-DE]" });

            asserted.Should().BeTrue();
        }

        [Fact]
        public static async Task Sets_CurrentCulture_to_last_directive_culture()
        {
            bool asserted = false;
            // Make sure we're invariant to begin with
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            var rootCommand = new RootCommand
            {
                Handler = CommandHandler.Create(() =>
                {
                    asserted = true;
                    CultureInfo.CurrentCulture.Name.Should().Be("nb-NO");
                })
            };
            var parser = new CommandLineBuilder(rootCommand)
                .UseCultureDirective()
                .Build();

            await parser.InvokeAsync(new[] { "[culture:de-DE]", "[culture:nb-NO]" });

            asserted.Should().BeTrue();
        }

        [Fact]
        public static async Task Sets_CurrentUICulture_to_directive_culture()
        {
            bool asserted = false;
            // Make sure we're invariant to begin with
            CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
            var rootCommand = new RootCommand
            {
                Handler = CommandHandler.Create(() =>
                {
                    asserted = true;
                    CultureInfo.CurrentUICulture.Name.Should().Be("de-DE");
                })
            };
            var parser = new CommandLineBuilder(rootCommand)
                .UseCultureDirective()
                .Build();

            await parser.InvokeAsync(new[] { "[uiculture:de-DE]" });
         
            asserted.Should().BeTrue();
        }

        [Fact]
        public static async Task Sets_CurrentUICulture_to_last_directive_culture()
        {
            bool asserted = false;
            // Make sure we're invariant to begin with
            CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
            var rootCommand = new RootCommand
            {
                Handler = CommandHandler.Create(() =>
                {
                    asserted = true;
                    CultureInfo.CurrentUICulture.Name.Should().Be("nb-NO");
                })
            };
            var parser = new CommandLineBuilder(rootCommand)
                .UseCultureDirective()
                .Build();

            await parser.InvokeAsync(new[] { "[uiculture:de-DE]", "[uiculture:nb-NO]" });

            asserted.Should().BeTrue();
        }

        [Fact]
        public static async Task Sets_CurrentCulture_to_invariant_culture()
        {
            bool asserted = false;
            // Make sure we're not invariant to begin with
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
            var rootCommand = new RootCommand
            {
                Handler = CommandHandler.Create(() =>
                {
                    asserted = true;
                    CultureInfo.CurrentCulture.Should().Be(CultureInfo.InvariantCulture);
                })
            };

            var parser = new CommandLineBuilder(rootCommand)
                .UseCultureDirective()
                .Build();

            await parser.InvokeAsync(new[] { "[invariantculture]" });

            asserted.Should().BeTrue();
        }

        [Fact]
        public static async Task Sets_CurrentUICulture_to_invariant_culture()
        {
            bool asserted = false;
            // Make sure we're not invariant to begin with
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
            var rootCommand = new RootCommand
            {
                Handler = CommandHandler.Create(() =>
                {
                    asserted = true;
                    CultureInfo.CurrentUICulture.Should().Be(CultureInfo.InvariantCulture);
                })
            };

            var parser = new CommandLineBuilder(rootCommand)
                .UseCultureDirective()
                .Build();

            await parser.InvokeAsync(new[] { "[invariantuiculture]" });

            asserted.Should().BeTrue();
        }
    }
}
