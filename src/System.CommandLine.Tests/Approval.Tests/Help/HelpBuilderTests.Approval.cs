// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;
using System.CommandLine.Help;
using ApprovalTests;


namespace System.CommandLine.Tests.Help
{
    public partial class HelpBuilderTests
    {
        [Fact]
        public void Help_describes_default_value_for_complex_approval_scenario()
        {
            var command = new RootCommand(description: "Test description");

            HelpBuilder helpBuilder = GetHelpBuilder(LargeMaxWidth);
            helpBuilder.Write(command);
            var output = _console.Out.ToString();
            Approvals.Verify(output);
        }
    }
}
