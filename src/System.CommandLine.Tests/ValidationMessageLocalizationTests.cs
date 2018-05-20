// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Builder;
using FluentAssertions;
using System.Linq;
using Xunit;

namespace System.CommandLine.Tests
{
    public class ValidationMessageLocalizationTests
    {

        [Fact]
        public void Default_validation_messages_can_be_replaced_in_order_to_add_localization_support()
        {
            var messages = new FakeValidationMessages("the-message");

            var builder = new ArgumentDefinitionBuilder();
            var commandDefinition = new CommandDefinition("the-command", "", symbolDefinitions: null, argumentDefinition: builder.ExactlyOne());
            var parser = new Parser(new ParserConfiguration(new[] { commandDefinition }, validationMessages: messages));
            var result = parser.Parse("the-command");

            result.Errors
                  .Select(e => e.Message)
                  .Should()
                  .Contain("the-message");
        }

        public class FakeValidationMessages : ValidationMessages
        {
            private readonly string message;

            public FakeValidationMessages(string message)
            {
                this.message = message;
            }

            public override string NoArgumentsAllowed(string option) => message;

            public override string CommandAcceptsOnlyOneArgument(string command, int argumentCount) => message;

            public override string FileDoesNotExist(string filePath) => message;

            public string CommandAcceptsOnlyOneSubcommand(string command, string subcommandsSpecified) => message;

            public override string OptionAcceptsOnlyOneArgument(string option, int argumentCount) => message;

            public override string RequiredArgumentMissingForCommand(string command) => message;

            public override string RequiredArgumentMissingForOption(string option) => message;

            public override string RequiredCommandWasNotProvided() => message;

            public override string UnrecognizedArgument(string unrecognizedArg, IReadOnlyCollection<string> allowedValues) => message;

            public override string UnrecognizedCommandOrArgument(string arg) => message;

            public override string UnrecognizedOption(string unrecognizedOption, IReadOnlyCollection<string> allowedValues) => message;
        }
    }
}
