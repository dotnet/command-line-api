// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

namespace System.CommandLine.StaticCompletions.Tests;

using System.CommandLine.StaticCompletions.Shells;

public class NushellShellProviderTests(ITestOutputHelper log)
{
    private IShellProvider provider = new NushellShellProvider();

    [Fact]
    public async Task GenericCompletions()
    {
        await provider.Verify(new("mycommand"), log);
    }
}
