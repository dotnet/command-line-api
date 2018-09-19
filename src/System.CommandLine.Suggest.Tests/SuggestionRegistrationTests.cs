using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Suggest.Tests
{
    
    public class SuggestionRegistrationTests
    {
        [Fact]
        public void Constructor_with_badly_formatted_completion_provider_throws()
        {
            Action action = () => new SuggestionRegistration("foo^^bar");
            action
                .Should()
                .Throw<ArgumentException>()
                .Where(item=>item.Message.StartsWith("Syntax for configuration of 'foo^^bar' is not of the format '<command>=<value>'"));
        }

    }
}
