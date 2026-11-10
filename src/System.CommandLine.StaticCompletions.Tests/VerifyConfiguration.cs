// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

namespace System.CommandLine.StaticCompletions.Tests;

using System.Runtime.CompilerServices;

public static class VerifyConfiguration
{
    [ModuleInitializer]
    public static void Initialize()
    {
        VerifyDiffPlex.Initialize(VerifyTests.DiffPlex.OutputType.Compact);

        if (Environment.GetEnvironmentVariable("CI") is string ci && ci.Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            Verifier.DerivePathInfo((sourceFile, projectDirectory, type, method) => new(
                directory: Path.Combine(Environment.CurrentDirectory, "snapshots"),
                typeName: type.Name,
                methodName: method.Name)
            );
        }

        EmptyFiles.FileExtensions.AddTextExtension("ps1");
        EmptyFiles.FileExtensions.AddTextExtension("nu");
        EmptyFiles.FileExtensions.AddTextExtension("fish");
    }
}
