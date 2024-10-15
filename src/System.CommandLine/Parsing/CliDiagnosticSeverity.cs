// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Parsing;

/// <summary>
/// Describes how severe a <see cref="CliDiagnostic"/> is."/>
/// </summary>
// Pattern based on: https://github.com/dotnet/roslyn/blob/1cca63b5d8ea170f8d8e88e1574aa3ebe354c23b/src/Compilers/Core/Portable/Diagnostic/DiagnosticSeverity.cs.
public enum CliDiagnosticSeverity
{
    /// <summary>
    /// Something that is not surfaced through normal means.
    /// </summary>
    Hidden = 0,

    /// <summary>
    /// Information that does not indicate a problem (i.e. not prescriptive).
    /// </summary>
    Info,

    /// <summary>
    /// Something suspicious but allowed.
    /// </summary>
    Warning,

    /// <summary>
    /// Something that is definitely wrong and needs fixing.
    /// </summary>
    Error
}
