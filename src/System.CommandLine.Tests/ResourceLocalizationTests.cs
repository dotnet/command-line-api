// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Parsing;
using FluentAssertions;
using System.Linq;
using Xunit;

namespace System.CommandLine.Tests
{
    public class ResourceLocalizationTests
    {
        [Fact]
        public void Default_validation_messages_can_be_replaced_in_order_to_add_localization_support()
        {
            var messages = new FakeLocalizationResources("the-message");

            var command = new Command("the-command")
            {
                new Argument<string>()
            };
            var parser = new Parser(new CommandLineConfiguration(command, resources: messages));
            var result = parser.Parse("the-command");

            result.Errors
                  .Select(e => e.Message)
                  .Should()
                  .Contain("the-message");
        }

        [Fact]
        public void Default_validation_messages_can_be_replaced_using_CommandLineBuilder_in_order_to_add_localization_support()
        {
            var messages = new FakeLocalizationResources("the-message");

            var parser = new CommandLineBuilder(new Command("the-command")
                         {
                             new Argument<string>()
                         })
                         .UseLocalizationResources(messages)
                         .Build();

            var result = parser.Parse("the-command");

            result.Errors
                  .Select(e => e.Message)
                  .Should()
                  .Contain("the-message");
        }

        public class FakeLocalizationResources : LocalizationResources
        {
            private readonly string message;

            public FakeLocalizationResources(string message)
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
