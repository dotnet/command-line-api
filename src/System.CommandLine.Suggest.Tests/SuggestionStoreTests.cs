// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Tests.Utility;
using FluentAssertions;
using Xunit.Abstractions;
using static System.Environment;

namespace System.CommandLine.Suggest.Tests
{
    public class SuggestionStoreTests : TestsWithTestApps
    {
        public SuggestionStoreTests(ITestOutputHelper output) : base(output)
        {
        }

        [ReleaseBuildOnlyFact]
        public void GetCompletions_obtains_suggestions_successfully()
        {
            var store = new SuggestionStore();
            var completions = store.GetCompletions(EndToEndTestApp.FullName, "[suggest:1] \"a\"", TimeSpan.FromSeconds(1));
            completions.Should().Be($"--apple{NewLine}--banana{NewLine}--durian{NewLine}");
        }
        
        [ReleaseBuildOnlyFact]
        public void GetCompletions_fails_to_obtain_suggestions_because_app_takes_too_long()
        {
            var store = new SuggestionStore();
            var appHangingTimeSpanArgument = TimeSpan.FromMilliseconds(2000).ToString();
            var completions = store
                .GetCompletions(WaitAndFailTestApp.FullName, appHangingTimeSpanArgument, TimeSpan.FromMilliseconds(1000));
            completions.Should().BeEmpty();
        }
        
        [ReleaseBuildOnlyFact]
        public void GetCompletions_fails_to_obtain_suggestions_because_app_exited_with_nonzero_code()
        {
            var store = new SuggestionStore();
            var appHangingTimeSpanArgument = TimeSpan.FromMilliseconds(0).ToString();
            var completions = store
                .GetCompletions(WaitAndFailTestApp.FullName, appHangingTimeSpanArgument, TimeSpan.FromMilliseconds(1000));
            completions.Should().BeEmpty();
        }
    }
}
