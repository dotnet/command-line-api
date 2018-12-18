// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Xunit;

namespace System.CommandLine.Suggest.Tests
{
    
    public class SuggestionRegistrationTests
    {
        [Fact]
        public void Constructor_with_badly_formatted_suggestion_provider_throws()
        {
            Action action = () => new RegistrationPair("foo^^bar");
            action
                .Should()
                .Throw<ArgumentException>()
                .Where(item=>item.Message.StartsWith("Syntax for configuration of 'foo^^bar' is not of the format '<command>=<value>'"));
        }

    }
}
