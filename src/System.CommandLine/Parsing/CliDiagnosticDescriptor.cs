// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace System.CommandLine.Parsing;

/// <summary>
/// Provides a description of a <see cref="CliDiagnostic"/>.
/// </summary>
public sealed class CliDiagnosticDescriptor
{
    public CliDiagnosticDescriptor(string id, string title, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] string messageFormat, CliDiagnosticSeverity severity, string? helpUri)
    {
        Id = id;
        Title = title;
        MessageFormat = messageFormat;
        Severity = severity;
        HelpUri = helpUri;
    }

    /// <summary>
    /// A unique identifier for the diagnostic.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// A short title describing the diagnostic.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// A composite format string, which can be passed to <see cref="string.Format(string, object[])"/> to create a message.
    /// </summary>
    [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
    public string MessageFormat { get; }

    /// <summary>
    /// The severity of the diagnostic.
    /// </summary>
    public CliDiagnosticSeverity Severity { get; }

    /// <summary>
    /// An optional hyperlink that provides more information about the diagnostic.
    /// </summary>
    public string? HelpUri { get; }
}
