// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Builder;
using System.IO;
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
            public string ResponseFileNotFound(string filePath) => message;
            public string ErrorReadingResponseFile(string filePath, IOException e) => message;
        }
    }
}
