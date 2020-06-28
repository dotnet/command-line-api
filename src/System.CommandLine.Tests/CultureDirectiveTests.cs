using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Globalization;
using System.Threading.Tasks;

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
                    Assert.Equal("de-DE", CultureInfo.CurrentCulture.Name);
                })
            };
            var parser = new CommandLineBuilder(rootCommand)
                .UseCultureDirective()
                .Build();

            await parser.InvokeAsync(new[] { "[culture:de-DE]" });

            Assert.True(asserted);
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
                    Assert.Equal("nb-NO", CultureInfo.CurrentCulture.Name);
                })
            };
            var parser = new CommandLineBuilder(rootCommand)
                .UseCultureDirective()
                .Build();

            await parser.InvokeAsync(new[] { "[culture:de-DE]", "[culture:nb-NO]" });

            Assert.True(asserted);
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
                    Assert.Equal("de-DE", CultureInfo.CurrentUICulture.Name);
                })
            };
            var parser = new CommandLineBuilder(rootCommand)
                .UseCultureDirective()
                .Build();

            await parser.InvokeAsync(new[] { "[uiculture:de-DE]" });
         
            Assert.True(asserted);
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
                    Assert.Equal("nb-NO", CultureInfo.CurrentUICulture.Name);
                })
            };
            var parser = new CommandLineBuilder(rootCommand)
                .UseCultureDirective()
                .Build();

            await parser.InvokeAsync(new[] { "[uiculture:de-DE]", "[uiculture:nb-NO]" });

            Assert.True(asserted);
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
                    Assert.Equal(CultureInfo.InvariantCulture, CultureInfo.CurrentCulture);
                })
            };

            var parser = new CommandLineBuilder(rootCommand)
                .UseCultureDirective()
                .Build();

            await parser.InvokeAsync(new[] { "[invariantculture]" });

            Assert.True(asserted);
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
                    Assert.Equal(CultureInfo.InvariantCulture, CultureInfo.CurrentUICulture);
                })
            };

            var parser = new CommandLineBuilder(rootCommand)
                .UseCultureDirective()
                .Build();

            await parser.InvokeAsync(new[] { "[invariantuiculture]" });

            Assert.True(asserted);
        }
    }
}
