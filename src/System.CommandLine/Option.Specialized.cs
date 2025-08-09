// Copyright (c) .NET Foundation and contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace System.CommandLine
{
#if NET7_0_OR_GREATER
    /// <summary>
    /// Option specialized for types implementing ISpanParsable{T}. Uses SpanParsableArgument{T} directly.
    /// </summary>
    public sealed class SpanParsableOption<T> : Option<T> where T : ISpanParsable<T>
    {
        public SpanParsableOption(string name, params string[] aliases)
            : base(name, aliases, new SpanParsableArgument<T>(name))
        {
        }
    }

    /// <summary>
    /// Option specialized for types implementing IParsable{T}. Uses ParsableArgument{T} directly.
    /// </summary>
    public sealed class ParsableOption<T> : Option<T> where T : IParsable<T>
    {
        public ParsableOption(string name, params string[] aliases)
            : base(name, aliases, new ParsableArgument<T>(name))
        {
        }
    }

    
    /// <summary>
    /// Option specialized for types implementing IParsable{T}. Uses ParsableArgument{T} directly.
    /// </summary>
    public sealed class ParsableListOption<T> : Option<List<T>> where T : IParsable<T>
    {
        public ParsableListOption(string name, params string[] aliases)
            : base(name, aliases, new ParsableListArgument<T>(name))
        {
        }
    }
#endif
}
