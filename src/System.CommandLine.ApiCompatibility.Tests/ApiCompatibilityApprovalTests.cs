// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApprovalTests;
using ApprovalTests.Reporters;
using Xunit;

namespace System.CommandLine.ApiCompatibility.Tests;

public class ApiCompatibilityApprovalTests
{
    [Fact(Skip = "This test to track API changes is turned off as we are aggressively changing the APO")]
    [UseReporter(typeof(DiffReporter))]
    public void System_CommandLine_api_is_not_changed()
    {
        var contract = ApiContract.GenerateContractForAssembly(typeof(ParseResult).Assembly);
        Approvals.Verify(contract);
    }
}