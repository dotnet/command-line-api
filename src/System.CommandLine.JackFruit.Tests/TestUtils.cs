using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine.Invocation;
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
            // Backwards but effective
            expected.Should().Contain(actual.Argument.Name);
        }

        public static void CheckArgument(Argument actual, string expected)
        {
            actual.Name.Should().Be(expected);
        }

        public static void CheckArgumentBindings(IEnumerable<(object Source, Argument Argument)> list, IEnumerable<string> expected)
        {
            list.Should().NotBeNull();
            expected.Count().Should().Be(list.Count());
            foreach (var s in expected)
            {
                list
                    .Any(x => x.Argument.Name == s)
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

        public static void CheckOptions(IEnumerable<SymbolBindingSide> optionBindings, params (string Name, Type Type)[] optionInfos)
        {
            optionBindings.Should().NotBeNull();
            optionBindings.Count().Should().Be(optionInfos.Length);
            CheckOptions(optionBindings
                        .Select(x => x.Symbol)
                        .OfType<Option>());
        }
    }
}
