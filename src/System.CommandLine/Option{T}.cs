// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;

namespace System.CommandLine
{
    public class Option<T> : Option
    {
        public Option(
            string alias,
            string? description = null,
            bool enforceTextMatch = true) 
            : base(new[] { alias }, description, new Argument<T>(), enforceTextMatch)
        { }

        public Option(
            string[] aliases,
            string? description = null,
            bool enforceTextMatch = true) 
            : base(aliases, description, new Argument<T>(), enforceTextMatch)
        { }

        public Option(
            string alias,
            ParseArgument<T> parseArgument,
            bool isDefault = false,
            string? description = null,
            bool enforceTextMatch = true) 
            : base(new[] { alias },
                  description, 
                  new Argument<T>(parseArgument ?? throw new ArgumentNullException(nameof(parseArgument)), isDefault),
                  enforceTextMatch)
        { }

        public Option(
            string[] aliases,
            ParseArgument<T> parseArgument,
            bool isDefault = false,
            string? description = null,
            bool enforceTextMatch = true) 
            : base(aliases,
                  description,
                  new Argument<T>(parseArgument ?? throw new ArgumentNullException(nameof(parseArgument)), isDefault),
                  enforceTextMatch)
        { }

        public Option(
            string alias,
            Func<T> getDefaultValue,
            string? description = null,
            bool enforceTextMatch = true) 
            : base(new[] { alias },
                  description, 
                  new Argument<T>(getDefaultValue ?? throw new ArgumentNullException(nameof(getDefaultValue))),
                  enforceTextMatch)
        { }

        public Option(
            string[] aliases,
            Func<T> getDefaultValue,
            string? description = null,
            bool enforceTextMatch = true) 
            : base(aliases,
                  description,
                  new Argument<T>(getDefaultValue ?? throw new ArgumentNullException(nameof(getDefaultValue))),
                  enforceTextMatch)
        { }

        internal Option(
            string[] aliases,
            Argument argument,
            string? description,
            bool enforceTextMatch = true)
            : base(aliases, description, argument, enforceTextMatch)
        { }
    }
}