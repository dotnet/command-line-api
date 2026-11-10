// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.CommandLine.StaticCompletions;

/// <summary>
/// Extensions for marking options or arguments require dynamic completions. Such symbols get special handling
/// in the static completion generation logic.
/// </summary>
public static class DynamicSymbolExtensions
{
    private static readonly object s_guard = new();

    /// <summary>
    /// The state that is used to track which symbols are dynamic.
    /// </summary>
    private static readonly Dictionary<Symbol, bool> s_dynamicSymbols = [];

    extension(Option option)
    {
        /// <summary>
        /// Indicates whether this option requires a dynamic call back into the application itself
        /// (via the built-in <c>[suggest]</c> directive) to compute completions at completion time.
        /// </summary>
        public bool IsDynamic
        {
            get
            {
                lock (s_guard)
                {
                    return s_dynamicSymbols.GetValueOrDefault(option, false);
                }
            }
            set
            {
                lock (s_guard)
                {
                    s_dynamicSymbols[option] = value;
                }
            }
        }
    }

    extension(Argument argument)
    {
        /// <summary>
        /// Indicates whether this argument requires a dynamic call back into the application itself
        /// (via the built-in <c>[suggest]</c> directive) to compute completions at completion time.
        /// </summary>
        public bool IsDynamic
        {
            get
            {
                lock (s_guard)
                {
                    return s_dynamicSymbols.GetValueOrDefault(argument, false);
                }
            }
            set
            {
                lock (s_guard)
                {
                    s_dynamicSymbols[argument] = value;
                }
            }
        }
    }
}
