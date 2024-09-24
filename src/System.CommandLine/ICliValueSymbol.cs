// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine;

/// <summary>
/// This is implemented only by <see cref="CliOption{T}"/> and <see cref="CliArgument{T}"/>, and allows
/// allows methods with a <see cref="CliValueSymbol"/> argument to apply constraints based on the
/// value type.
/// </summary>
/// <typeparam name="TValue">The value type</typeparam>
public interface ICliValueSymbol<TValue>
{
}
