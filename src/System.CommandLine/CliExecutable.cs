// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Reflection;

namespace System.CommandLine;

//TODO: cull unused member, consider making public again
/// <summary>
/// Static helpers for determining information about the CLI executable.
/// </summary>
internal static class CliExecutable
{
    private static Assembly? _assembly;
    private static string? _executablePath;
    private static string? _executableName;
    private static string? _executableVersion;

    internal static Assembly GetAssembly()
        => _assembly ??= (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly());

    /// <summary>
    /// The name of the currently running executable.
    /// </summary>
    public static string ExecutableName
        => _executableName ??= Path.GetFileNameWithoutExtension(ExecutablePath).Replace(" ", "");

    /// <summary>
    /// The path to the currently running executable.
    /// </summary>
    public static string ExecutablePath => _executablePath ??= Environment.GetCommandLineArgs()[0];

    internal static string ExecutableVersion => _executableVersion ??= GetExecutableVersion();

    private static string GetExecutableVersion()
    {
        var assembly = GetAssembly();

        var assemblyVersionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();

        if (assemblyVersionAttribute is null)
        {
            return assembly.GetName().Version?.ToString() ?? "";
        }
        else
        {
            return assemblyVersionAttribute.InformationalVersion;
        }
    }
}
