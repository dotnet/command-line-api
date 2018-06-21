using System.CommandLine.Builder;
using System.IO;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    public class ResponseFileTests : IDisposable
    {
        private string FilePath { get; }

        public ResponseFileTests()
        {
            FilePath = Path.GetTempFileName();
        }

        public void Dispose()
        {
            File.Delete(FilePath);
        }

        [Fact]
        public void When_response_file_specified_it_loads_arguments_from_response_file()
        {
            using (var s = new StreamWriter(FilePath))
            {
                s.Write("--flag");
            }
            var result = new Parser(new OptionDefinition(
                    "--flag",
                    ""))
                .Parse("@" + FilePath);
            result.HasOption("--flag").Should().BeTrue();
        }

        [Fact]
        public void When_response_file_specified_it_loads_arguments_with_blank_line_from_response_file()
        {
            using (var s = new StreamWriter(FilePath))
            {
                s.WriteLine("--flag");
                s.WriteLine("");
                s.WriteLine("--flag2");
                s.WriteLine("123");
            }
            var result = new CommandLineBuilder()
                .AddOption("--flag", "")
                .AddOption("--flag2", "", a => a.ParseArgumentsAs<int>())
                .Build()
                .Parse("@" + FilePath);
            result.HasOption("--flag").Should().BeTrue();
            result.ValueForOption("--flag2").Should().Be(123);
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void When_response_file_specified_it_loads_arguments_with_comment_lines_from_response_file()
        {
            using (var s = new StreamWriter(FilePath))
            {
                s.WriteLine("# comment one");
                s.WriteLine("--flag");
                s.WriteLine("# comment two");
                s.WriteLine("#");
                s.WriteLine(" # comment two");
                s.WriteLine("--flag2");
            }
            var result = new CommandLineBuilder()
                .AddOption("--flag", "")
                .AddOption("--flag2", "")
                .Build()
                .Parse("@" + FilePath);
            result.HasOption("--flag").Should().BeTrue();
            result.HasOption("--flag2").Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void When_response_file_does_not_exist_adds_to_errors()
        {
            File.Delete(FilePath);
            var result = new CommandLineBuilder()
                .AddOption("--flag", "")
                .AddOption("--flag2", "")
                .Build()
                .Parse("@" + FilePath);
            result.HasOption("--flag").Should().BeFalse();
            result.HasOption("--flag2").Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.Single().Message.Should().Be($"Response file not found '{FilePath}'");
        }

        [Fact]
        public void When_response_filepath_is_not_specified_error_is_returned()
        {
            var result = new CommandLineBuilder()
                .AddOption("--flag", "")
                .AddOption("--flag2", "")
                .Build()
                .Parse("@");
            result.HasOption("--flag").Should().BeFalse();
            result.HasOption("--flag2").Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.Single().Message.Should().Be("Unrecognized command or argument '@'");
        }

        [Fact]
        public void When_response_file_cannot_be_read_specified_error_is_returned()
        {
            using (File.Open(FilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                var result = new CommandLineBuilder()
                    .AddOption("--flag", "")
                    .AddOption("--flag2", "")
                    .Build()
                    .Parse("@" + FilePath);
                result.HasOption("--flag").Should().BeFalse();
                result.HasOption("--flag2").Should().BeFalse();
                result.Errors.Should().HaveCount(1);
                result.Errors.Single().Message.Should().StartWith($"Error reading response file '{FilePath}'");
            }
        }

        [Theory]
        [InlineData("--flag \"first value\" --flag2 123")]
        [InlineData("--flag:\"first value\" --flag2:123")]
        [InlineData("--flag=\"first value\" --flag2=123")]
        public void When_response_file_parse_as_space_separated_returns_expected_values(string input)
        {
            using (var s = new StreamWriter(FilePath))
            {
                s.WriteLine(input);
            }

            var result = new CommandLineBuilder()
                .AddOption("--flag", "", a => a.ExactlyOne())
                .AddOption("--flag2", "", a=> a.ParseArgumentsAs<int>())
                .ParseResponseFileAs(ResponseFileHandling.ParseArgsAsSpaceSeparated)
                .Build()
                .Parse("@" + FilePath);
            result.ValueForOption("--flag").Should().Be("first value");
            result.ValueForOption("--flag2").Should().Be(123);
        }

        [Fact]
        public void When_response_file_processing_is_disabled_returns_response_file_name_as_argument()
        {
            var result = new CommandLineBuilder()
                .AddOption("--flag", "")
                .AddOption("--flag2", "")
                .ParseResponseFileAs(ResponseFileHandling.Disabled)
                .Build()
                .Parse($"--flag @{FilePath} --flag2");
            result.HasOption("--flag").Should().BeTrue();
            result.HasOption("--flag2").Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors.Single().Message.Should().Be($"Unrecognized command or argument '@{FilePath}'");
        }
    }
}
