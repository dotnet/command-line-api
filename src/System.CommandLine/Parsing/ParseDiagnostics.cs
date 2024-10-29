// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Parsing;

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
