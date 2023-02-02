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
            //var messages = new FakeLocalizationResources("the-message");

            //var command = new Command("the-command")
            //{
            //    new Argument<string>()
            //};
            //var parser = new Parser(new CommandLineConfiguration(command, resources: messages));
            //var result = parser.Parse("the-command");

            //result.Errors
            //      .Select(e => e.Message)
            //      .Should()
            //      .Contain("the-message");
        }

        [Fact]
        public void Default_validation_messages_can_be_replaced_using_CommandLineBuilder_in_order_to_add_localization_support()
        {
            //var messages = new FakeLocalizationResources("the-message");

            //var parser = new CommandLineBuilder(new Command("the-command")
            //             {
            //                 new Argument<string>()
            //             })
            //             .UseLocalizationResources(messages)
            //             .Build();

            //var result = parser.Parse("the-command");

            //result.Errors
            //      .Select(e => e.Message)
            //      .Should()
            //      .Contain("the-message");
        }
    }
}
