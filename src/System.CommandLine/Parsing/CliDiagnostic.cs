// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace System.CommandLine.Parsing;

/*
 * Pattern based on: 
 * https://github.com/mhutch/MonoDevelop.MSBuildEditor/blob/main/MonoDevelop.MSBuild/Analysis/MSBuildDiagnostic.cs
 * https://github.com/mhutch/MonoDevelop.MSBuildEditor/blob/main/MonoDevelop.MSBuild/Analysis/MSBuildDiagnosticDescriptor.cs
 * https://github.com/dotnet/roslyn/blob/main/src/Compilers/Core/Portable/Diagnostic/DiagnosticDescriptor.cs
 * https://github.com/dotnet/roslyn/blob/main/src/Compilers/Core/Portable/Diagnostic/Diagnostic.cs
 * https://docs.oasis-open.org/sarif/sarif/v2.1.0/errata01/os/sarif-v2.1.0-errata01-os-complete.html#_Toc141791086
 */
internal static class ParseDiagnostics
{
    public const string DirectiveIsNotDefinedId = "CMD0001";
    public static readonly CliDiagnosticDescriptor DirectiveIsNotDefined =
        new(
            DirectiveIsNotDefinedId,
            //TODO: use localized strings
            "Directive is not defined",
            "The directive '{0}' is not defined.",
            CliDiagnosticSeverity.Error,
            null);
}

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

    public string Id { get; }
    public string Title { get; }
    [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
    public string MessageFormat { get; }
    public CliDiagnosticSeverity Severity { get; }
    public string? HelpUri { get; }
}

public enum CliDiagnosticSeverity
{
    Hidden = 0,
    Info,
    Warning,
    Error
}

/// <summary>
/// Describes an error that occurs while parsing command line input.
/// </summary>
public sealed class CliDiagnostic
{
    // TODO: reevaluate whether we should be exposing a SymbolResult here
    // TODO: Rename to CliError

    /// <summary>
    /// Initializes a new instance of the <see cref="CliDiagnostic"/> class.
    /// </summary>
    /// <param name="descriptor">Contains information about the error.</param>
    /// <param name="messageArgs">The arguments to be passed to the <see cref="CliDiagnosticDescriptor.MessageFormat"/> in the <paramref name="descriptor"/>.</param>
    /// <param name="properties">Properties to be associated with the diagnostic.</param>
    /// <param name="cliSymbolResult">Contains information about a single value entered.</param>
    /// <param name="location">The location of the error.</param>
    public CliDiagnostic(
        CliDiagnosticDescriptor descriptor,
        object?[]? messageArgs,
        ImmutableDictionary<string, object>? properties = null,
        CliSymbolResult? cliSymbolResult = null,
        Location? location = null)
    {
        Descriptor = descriptor;
        MessageArgs = messageArgs;
        Properties = properties;
    }

    /// <summary>
    /// Gets a message to explain the error to a user.
    /// </summary>
    public string Message
    {
        get
        {
            if (MessageArgs is not null)
            {
                return string.Format(Descriptor.MessageFormat, MessageArgs);
            }
            return Descriptor.MessageFormat;
        }
    }

    public ImmutableDictionary<string, object>? Properties { get; }

    public CliDiagnosticDescriptor Descriptor { get; }

    public object?[]? MessageArgs { get; }

    public CliSymbolResult? CliSymbolResult { get; }

    /// <inheritdoc />
    public override string ToString() => Message;
}
