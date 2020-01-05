﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
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

            var command = new Command("the-command")
            {
                new Argument
                {
                    Arity = ArgumentArity.ExactlyOne
                }
            };
            var parser = new Parser(new CommandLineConfiguration(new[] { command }, validationMessages: messages));
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

            public override string ExpectsOneArgument(SymbolResult symbolResult) => message;

            public override string FileDoesNotExist(string filePath) => message;

            public override string RequiredArgumentMissing(SymbolResult symbolResult) => message;

            public override string RequiredCommandWasNotProvided() => message;

            public override string UnrecognizedArgument(string unrecognizedArg, IReadOnlyCollection<string> allowedValues) => message;

            public override string UnrecognizedCommandOrArgument(string arg) => message;
        }
    }
}
