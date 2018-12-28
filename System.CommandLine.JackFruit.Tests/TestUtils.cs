using System.Collections.Generic;
using System.Linq;
using FluentAssertions;

namespace System.CommandLine.JackFruit.Tests
{
    internal class TestUtils
    {

        public static void CheckAliasList(IEnumerable<string> actual, IEnumerable<string> expected)
        {
            actual.Should().NotBeNull();
            expected.Count().Should().Be(actual.Count());
            foreach (var s in expected)
            {
                actual.Should().Contain(s);
            }
        }

        public static void CheckArguments(Command actual, IEnumerable<string> expected)
        {
            CheckArguments(new List<Argument>() { actual.Argument }, expected);
        }

        public static void CheckArguments(IEnumerable<Argument> actual, IEnumerable<string> expected)
        {
            actual.Should().NotBeNull();
            expected.Count().Should().Be(actual.Count());
            foreach (var s in expected)
            {
                actual
                   .Any(x => x.Name == s)
                   .Should().BeTrue();
            }
        }

        public static void CheckHelp(string actual, string expectedStart)
        {
            if (string.IsNullOrWhiteSpace(expectedStart))
            {
                actual.Should().BeNullOrWhiteSpace();
                return;
            }
            actual.Should().NotBeNullOrWhiteSpace();
            if (expectedStart.Length < 15)
            {
                actual.Should().Be(expectedStart);
                return;
            }
            actual.StartsWith(expectedStart).Should().BeTrue();
        }

        public static void CheckSubCommands(Command command, params string[] subCommandNames)
        {
            command.Should().NotBeNull();
            var childCommands = command.Children.OfType<Command>();
            CheckSubCommands(childCommands, subCommandNames);
        }

        public static void CheckSubCommands(IEnumerable<Command> childCommands, params string[] subCommandNames)
        {
            childCommands.Should().NotBeNull();
            childCommands.Count().Should().Be(subCommandNames.Length);
            foreach (var cmdName in subCommandNames)
            {
                childCommands.Any(x => x.Name == cmdName).Should().BeTrue();
            }
        }

        public static void CheckOptions(Command command, params (string Name, Type Type)[] optionInfos)
        {
            var childOptions = command.Children.OfType<Option>();
            CheckOptions(childOptions, optionInfos);
        }

        public static void CheckOptions(IEnumerable<Option> options, params (string Name, Type Type)[] optionInfos)
        {
            options.Should().NotBeNull();
            options.Count().Should().Be(optionInfos.Length);
            foreach (var opt in optionInfos)
            {
                var option = options.Where(x => x.Name == opt.Name).SingleOrDefault();
                option.Should().NotBeNull();
                option.Argument.ArgumentType.Should().Be(opt.Type);
            }
        }
    }
}
