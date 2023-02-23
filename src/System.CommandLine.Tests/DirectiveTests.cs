// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    public class DirectiveTests
    {
        [Fact]
        public void Directives_should_not_be_considered_as_unmatched_tokens_when_they_are_enabled()
        {
            RootCommand root = new () { new Option<bool>("-y") };
            CommandLineBuilder builder = new (root);
            builder.Directives.Add(new ("some"));

            var result = root.Parse($"{RootCommand.ExecutableName} [parse] -y", builder.Build());

            result.UnmatchedTokens.Should().BeEmpty();
        }

        [Fact]
        public void Raw_tokens_still_hold_directives()
        {
            Directive directive = new ("parse");

            ParseResult result = Parse(new Option<bool>("-y"), directive, "[parse] -y");

            result.FindResultFor(directive).Should().NotBeNull();
            result.Tokens.Should().Contain(t => t.Value == "[parse]");
        }

        [Fact]
        public void Multiple_directives_are_allowed()
        {
            RootCommand root = new() { new Option<bool>("-y") };
            Directive parseDirective = new ("parse");
            Directive suggestDirective = new ("suggest");
            CommandLineBuilder builder = new(root);
            builder.Directives.Add(parseDirective);
            builder.Directives.Add(suggestDirective);

            var result = root.Parse("[parse] [suggest] -y", builder.Build());

            result.FindResultFor(parseDirective).Should().NotBeNull();
            result.FindResultFor(suggestDirective).Should().NotBeNull();
        }

        [Fact]
        public void Directives_must_be_the_first_argument()
        {
            Directive directive = new("parse");

            ParseResult result = Parse(new Option<bool>("-y"), directive, "-y [parse]");

            result.FindResultFor(directive).Should().BeNull();
        }

        [Theory]
        [InlineData("[key:value]", "key", "value")]
        [InlineData("[key:value:more]", "key", "value:more")]
        [InlineData("[key:]", "key", "")]
        public void Directives_can_have_a_value_which_is_everything_after_the_first_colon(
            string wholeText,
            string key,
            string expectedValue)
        {
            Directive directive = new(key);

            ParseResult result = Parse(new Option<bool>("-y"), directive, $"{wholeText} -y");

            result.FindResultFor(directive).Values.Single().Should().Be(expectedValue);
        }

        [Fact]
        public void Directives_without_a_value_specified_have_no_values()
        {
            Directive directive = new("parse");

            ParseResult result = Parse(new Option<bool>("-y"), directive, "[parse] -y");

            result.FindResultFor(directive).Values.Should().BeEmpty();
        }

        [Theory]
        [InlineData("[]")]
        [InlineData("[:value]")]
        public void Directives_must_have_a_non_empty_key(string directive)
        {
            Option<bool> option = new ("-a");
            RootCommand root = new () { option };

            var result = root.Parse($"{directive} -a");

            result.UnmatchedTokens.Should().Contain(directive);
        }

        [Theory]
        [InlineData("[par se]")]
        [InlineData("[ parse]")]
        [InlineData("[parse ]")]
        public void Directives_cannot_contain_spaces(string value)
        {
            Action create = () => new Directive(value);

            create.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void When_a_directive_is_specified_more_than_once_then_its_values_are_aggregated()
        {
            Directive directive = new("directive");

            ParseResult result = Parse(new Option<bool>("-a"), directive, "[directive:one] [directive:two] -a");

            result.FindResultFor(directive).Values.Should().BeEquivalentTo("one", "two");
        }

        [Fact]
        public void When_directives_are_not_enabled_they_are_treated_as_regular_tokens()
        {
            var config = new CommandLineConfiguration(
                    new RootCommand
                    {
                        new Argument<List<string>>()
                    });

            var result = config.RootCommand.Parse("[hello]", config);

            result.CommandResult
                  .Tokens
                  .Select(t => t.Value)
                  .Should()
                  .BeEquivalentTo("[hello]");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Directive_can_restore_the_state_after_running_continuation(bool async)
        {
            const string plCulture = "pl-PL", enUsCulture = "en-US";
            const string envVarName = "uniqueName", envVarValue = "just";

            var before = CultureInfo.CurrentUICulture;

            try
            {
                CultureInfo.CurrentUICulture = new(enUsCulture);

                bool invoked = false;
                Option<bool> option = new("-a");
                RootCommand root = new() { option };
                CommandLineBuilder builder = new(root);
                builder.Directives.Add(new EnvironmentVariablesDirective());
                builder.Directives.Add(new CultureDirective());
                root.SetHandler(ctx =>
                {
                    invoked = true;
                    CultureInfo.CurrentUICulture.Name.Should().Be(plCulture);
                    Environment.GetEnvironmentVariable(envVarName).Should().Be(envVarValue);
                });

                if (async)
                {
                    await builder.Build().InvokeAsync($"[culture:{plCulture}] [env:{envVarName}={envVarValue}]");
                }
                else
                {
                    builder.Build().Invoke($"[culture:{plCulture}] [env:{envVarName}={envVarValue}]");
                }

                invoked.Should().BeTrue();
            }
            finally
            {
                CultureInfo.CurrentUICulture = before;
            }
        }

        private static ParseResult Parse(Option option, Directive directive, string commandLine)
        {
            RootCommand root = new() { option };
            CommandLineBuilder builder = new(root);
            builder.Directives.Add(directive);

            return root.Parse(commandLine, builder.Build());
        }

        private sealed class CultureDirective : Directive
        {
            public CultureDirective() : base("culture")
            {
                SetSynchronousHandler((ctx, next) =>
                {
                    CultureInfo cultureBefore = CultureInfo.CurrentUICulture;

                    try
                    {
                        string cultureName = ctx.ParseResult.FindResultFor(this).Values.Single();

                        CultureInfo.CurrentUICulture = new CultureInfo(cultureName);

                        next?.Invoke(ctx);
                    }
                    finally
                    {
                        CultureInfo.CurrentUICulture = cultureBefore;
                    }
                });
                SetAsynchronousHandler(async (ctx, next, ct) =>
                {
                    CultureInfo cultureBefore = CultureInfo.CurrentUICulture;

                    try
                    {
                        string cultureName = ctx.ParseResult.FindResultFor(this).Values.Single();

                        CultureInfo.CurrentUICulture = new CultureInfo(cultureName);

                        await next?.InvokeAsync(ctx, ct);
                    }
                    finally
                    {
                        CultureInfo.CurrentUICulture = cultureBefore;
                    }
                });
            }
        }

    }
}