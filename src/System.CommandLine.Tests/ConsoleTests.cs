using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    public abstract class ConsoleTests
    {
        protected abstract IConsole GetConsole();

        [Fact]
        public void Setting_CursorLeft_below_zero_throws()
        {
            var console = GetConsole();

            console.Invoking(c => c.CursorLeft = -1)
                   .Should()
                   .Throw<ArgumentOutOfRangeException>()
                   .WithMessage($"The value must be greater than or equal to zero and less than the console's buffer size in that dimension.{Environment.NewLine}Parameter name: left{Environment.NewLine}Actual value was -1.");
        }

        [Fact]
        public void Setting_CursorTop_below_zero_throws()
        {
            var console = GetConsole();

            console.Invoking(c => c.CursorTop = -1)
                   .Should()
                   .Throw<ArgumentOutOfRangeException>()
                   .WithMessage($"The value must be greater than or equal to zero and less than the console's buffer size in that dimension.{Environment.NewLine}Parameter name: top{Environment.NewLine}Actual value was -1.");
        }

    }
}