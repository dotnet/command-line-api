// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Parsing;

/// <summary>
/// Performs custom parsing of an argument.
/// </summary>
/// <typeparam name="T">The type which the argument is to be parsed as.</typeparam>
/// <param name="result">The argument result.</param>
/// <returns>The parsed value.</returns>
/// <remarks>Validation errors can be returned by setting <see cref="SymbolResult.ErrorMessage"/>.</remarks>
public delegate T ParseArgument<out T>(ArgumentResult result);