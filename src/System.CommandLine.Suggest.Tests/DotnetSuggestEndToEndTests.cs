using System.CommandLine.Invocation;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Suggest.Tests
{
    public class DotnetSuggestEndToEndTests
    {
        [Fact]
        public async Task Test_app_supplies_completions()
        {
            var (exitCode, stdOut, stdErr) = await Process.ExecuteAsync(
                                                 "EndToEndTestApp",
                                                 "[suggest] a");

            stdOut.Should().Be($"--apple{Environment.NewLine}--banana{Environment.NewLine}--durian");
        }

        [Fact]
        public async Task dotnet_suggest_provides_completions_for_app()
        {
            var (exitCode, stdOut, stdErr) = await Process.ExecuteAsync(
                                                 "dotnet-suggest",
                                                 $"list -e EndToEndTestApp");

            stdOut.Should().Be($"--apple{Environment.NewLine}--banana{Environment.NewLine}--durian");
        }
    }
}
