using System;
using System.Text;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using static System.Environment;

namespace Microsoft.DotNet.Cli.CommandLine.SampleParsers.Dotnet
{
    public static class AssertionExtensions
    {
        public static AndConstraint<StringAssertions> MatchLineByLine(
            this StringAssertions assertions,
            string expectedText,
            string because = null)
        {
            var expectedLines = expectedText
                .Split(new[] { '\r', '\n' },
                       StringSplitOptions.RemoveEmptyEntries);
            var actualLines = assertions
                .Subject
                .Split(new[] { '\r', '\n' },
                       StringSplitOptions.RemoveEmptyEntries);

            var sb = new StringBuilder();

            var matchedCount = 0;
            var differedCount = 0;

            for (var i = 0; i < actualLines.Length; i++)
            {
                var expected = expectedLines[i];
                var actual = actualLines[i];

                if (expected != actual)
                {
                    differedCount++;
                    sb.AppendLine("EXPECTED--> " + expected);
                    sb.AppendLine("ACTUAL  --> " + actual);
                    sb.AppendLine();
                }
                else
                {
                    matchedCount++;
                    sb.AppendLine("           " + actual);
                }
            }

            if (differedCount > 0)
            {
                throw new AssertionFailedException($"Text differs on {differedCount} out of {actualLines.Length} lines.{NewLine}{NewLine}{sb}");
            }
            return new AndConstraint<StringAssertions>(assertions);
        }
    }
}