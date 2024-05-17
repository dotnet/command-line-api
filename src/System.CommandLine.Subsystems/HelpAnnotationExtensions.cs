// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Subsystems;
using System.CommandLine.Subsystems.Annotations;

namespace System.CommandLine;

public static class HelpAnnotationExtensions
{
    /// <summary>
    /// Sets the help description on the <paramref name="symbol"/>
    /// </summary>
    /// <typeparam name="TSymbol">The type of the symbol</typeparam>
    /// <param name="symbol">The symbol</param>
    /// <param name="description">The help description for the symbol</param>
    /// <returns>The <paramref name="symbol">, to enable fluent construction of symbols with annotations.</returns>
    public static TSymbol WithDescription<TSymbol> (this TSymbol symbol, string description) where TSymbol : CliSymbol
    {
        symbol.SetDescription(description);
        return symbol;
    }


    /// <summary>
    /// Sets the help description on the <paramref name="symbol"/>
    /// </summary>
    /// <typeparam name="TSymbol">The type of the symbol</typeparam>
    /// <param name="symbol">The symbol</param>
    /// <param name="description">The help description for the symbol</param>
    public static void SetDescription<TSymbol>(this TSymbol symbol, string description) where TSymbol : CliSymbol
    {
        symbol.SetAnnotation(HelpAnnotations.Description, description);
    }

    /// <summary>
    /// Get the help description on the <paramref name="symbol"/>
    /// </summary>
    /// <typeparam name="TSymbol">The type of the symbol</typeparam>
    /// <param name="symbol">The symbol</param>
    /// <returns>The symbol description if any, otherwise <see langword="null"/></returns>
    /// <remarks>
    /// This is intended to be called by CLI authors. Subsystems should instead call <see cref="HelpSubsystem.TryGetDescription(CliSymbol, out string?)"/>,
    /// values from the subsystem's <see cref="IAnnotationProvider"/>.
    /// </remarks>
    public static string? GetDescription<TSymbol>(this TSymbol symbol) where TSymbol : CliSymbol
    {
        return symbol.GetAnnotationOrDefault(HelpAnnotations.Description);
    }
}
