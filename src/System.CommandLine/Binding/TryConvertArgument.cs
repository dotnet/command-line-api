// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;

namespace System.CommandLine.Binding
{
    internal delegate bool TryConvertArgument(
        ArgumentResult argumentResult,
        out object value);

    /// <summary>
    /// Converts an <see cref="ArgumentResult"/> into an instance of <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to convert to.</typeparam>
    /// <param name="argumentResult">The <see cref="ArgumentResult"/> representing parsed input to be converted.</param>
    /// <param name="value">The converted result.</param>
    /// <returns>True if the conversion is successful; otherwise, false.</returns>
    /// <remarks>Validation errors can be returned by setting <see cref="SymbolResult.ErrorMessage"/>.</remarks>
    public delegate bool TryConvertArgument<T>(
        ArgumentResult argumentResult,
        out T value);
}