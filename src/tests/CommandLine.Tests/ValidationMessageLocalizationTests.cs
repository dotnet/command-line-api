// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using FluentAssertions;
using System.Linq;
using Xunit;
using static Microsoft.DotNet.Cli.CommandLine.Create;

namespace Microsoft.DotNet.Cli.CommandLine.Tests
{
    public class ValidationMessageLocalizationTests : IDisposable
    {
        public void Dispose()
        {
            ValidationMessages.Current = null;
        }

        [Fact]
        public void Default_validation_messages_can_be_replaced_in_order_to_add_localization_support()
        {
            ValidationMessages.Current = new FakeValidationMessages("the-message");

            var builder = new ArgumentRuleBuilder();
            var result = Command("the-command", "", builder.ExactlyOne()).Parse("the-command");

            result.Errors
                  .Select(e => e.Message)
                  .Should()
                  .Contain("the-message");
        }

        [Fact]
        public void When_an_IValidationMessages_implementation_returns_null_then_the_default_message_is_used()
        {
            ValidationMessages.Current = new FakeValidationMessages(null);

            var builder = new ArgumentRuleBuilder();
            var result = Command("the-command", "", ArgumentsRule.None).Parse("the-command an-argument");

            result.Errors
                  .Select(e => e.Message)
                  .Should()
                  .Contain("Unrecognized command or argument 'an-argument'");
        }

        [Fact]
        public void When_an_IValidationMessages_implementation_returns_whitespace_then_the_default_message_is_used()
        {
            ValidationMessages.Current = new FakeValidationMessages("  ");

            var builder = new ArgumentRuleBuilder();
            var result = Command("outer", "",
                                 Command("inner", "", ArgumentsRule.None)).Parse("outer");

            result.Errors
                  .Select(e => e.Message)
                  .Should()
                  .Contain("Required argument missing for command: outer");
        }

        public class FakeValidationMessages : IValidationMessages
        {
            private readonly string message;

            public FakeValidationMessages(string message)
            {
                this.message = message;
            }

            public string NoArgumentsAllowed(string option) => message;

            public string CommandAcceptsOnlyOneArgument(string command, int argumentCount) => message;

            public string FileDoesNotExist(string filePath) => message;

            public string CommandAcceptsOnlyOneSubcommand(string command, string subcommandsSpecified) => message;

            public string OptionAcceptsOnlyOneArgument(string option, int argumentCount) => message;

            public string RequiredArgumentMissingForCommand(string command) => message;

            public string RequiredArgumentMissingForOption(string option) => message;

            public string RequiredCommandWasNotProvided() => message;

            public string UnrecognizedArgument(string unrecognizedArg, IReadOnlyCollection<string> allowedValues) => message;

            public string UnrecognizedCommandOrArgument(string arg) => message;

            public string UnrecognizedOption(string unrecognizedOption, IReadOnlyCollection<string> allowedValues) => message;
        }
    }
}
