// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;

namespace System.CommandLine.Parsing
{
    /*
     * Pattern based on: 
     * https://github.com/mhutch/MonoDevelop.MSBuildEditor/blob/main/MonoDevelop.MSBuild/Analysis/MSBuildDiagnostic.cs
     * https://github.com/mhutch/MonoDevelop.MSBuildEditor/blob/main/MonoDevelop.MSBuild/Analysis/MSBuildDiagnosticDescriptor.cs
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
        public CliDiagnosticDescriptor(string id, string title, string messageFormat, CliDiagnosticSeverity severity, string? helpUri)
        {
            Id = id;
            Title = title;
            MessageFormat = messageFormat;
            Severity = severity;
            HelpUri = helpUri;
        }

        public string Id { get; }
        public string Title { get; }
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
        // TODO: add position
        // TODO: reevaluate whether we should be exposing a SymbolResult here
        // TODO: Rename to CliError

        /// <summary>
        /// Initializes a new instance of the <see cref="CliDiagnostic"/> class.
        /// </summary>
        /// <param name="descriptor">Contains information about the error.</param>
        /// <param name="messageArgs">The arguments to be passed to the <see cref="CliDiagnosticDescriptor.MessageFormat"/> in the <paramref name="descriptor"/>.</param>
        /// <param name="properties">Properties to be associated with the diagnostic.</param>
        /// <param name="symbolResult">The symbol result detailing the symbol that failed to parse and the tokens involved.</param>
        /// <param name="location">The location of the error.</param>
        public CliDiagnostic(
            CliDiagnosticDescriptor descriptor,
            string[] messageArgs,
            ImmutableDictionary<string, object>? properties = null,
            SymbolResult? symbolResult = null,
            Location? location = null)
        {
            //if (string.IsNullOrWhiteSpace(message))
            //{
            //    throw new ArgumentException("Value cannot be null or whitespace.", nameof(message));
            //}

            Message = string.Format(descriptor.MessageFormat, messageArgs);
            SymbolResult = symbolResult;
        }

        /// <summary>
        /// Gets a message to explain the error to a user.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets the symbol result detailing the symbol that failed to parse and the tokens involved.
        /// </summary>
        public SymbolResult? SymbolResult { get; }

        /// <inheritdoc />
        public override string ToString() => Message;
    }
}
