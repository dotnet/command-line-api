// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Subsystems.Annotations;

namespace System.CommandLine;

public static class HelpAnnotationExtensions
{
    public static TSymbol WithDescription<TSymbol> (this TSymbol symbol, string description) where TSymbol : CliSymbol
    {
        symbol.SetDescription(description);
        return symbol;
    }

    public static void SetDescription<TSymbol>(this TSymbol symbol, string description) where TSymbol : CliSymbol
    {
        symbol.SetAnnotation(HelpAnnotations.Description, description);
    }

    public static string? GetDescription<TSymbol>(this TSymbol symbol) where TSymbol : CliSymbol
    {
        return symbol.GetAnnotationOrDefault(HelpAnnotations.Description);
    }
}
