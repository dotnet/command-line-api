// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using VerifyXunit;
using Xunit;

namespace System.CommandLine.ApiCompatibility.Tests;

public class ApiCompatibilityTests
{
    [Fact]
    public Task System_CommandLine_api_is_not_changed()
    {
        var contract = ApiContract.GenerateContractForAssembly(typeof(ParseResult).Assembly);
        return Verifier.Verify(contract);
    }
}