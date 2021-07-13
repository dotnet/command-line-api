// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.CommandLine.Parsing;

namespace System.CommandLine
{
    public class Option<T> : Option, IValueDescriptor<T>
    {
        public Option(
            string alias,
            string? description = null) 
            : base(new[] { alias }, description, new Argument<T>())
        { }

        public Option(
            string[] aliases,
            string? description = null) 
            : base(aliases, description, new Argument<T>())
        { }

        public Option(
            string alias,
            ParseArgument<T> parseArgument,
            bool isDefault = false,
            string? description = null) 
            : base(new[] { alias }, description, 
                  new Argument<T>(parseArgument ?? throw new ArgumentNullException(nameof(parseArgument)), isDefault))
        { }

        public Option(
            string[] aliases,
            ParseArgument<T> parseArgument,
            bool isDefault = false,
            string? description = null) 
            : base(aliases, description, new Argument<T>(parseArgument ?? throw new ArgumentNullException(nameof(parseArgument)), isDefault))
        { }

        public Option(
            string alias,
            Func<T> getDefaultValue,
            string? description = null) 
            : base(new[] { alias }, description, 
                  new Argument<T>(getDefaultValue ?? throw new ArgumentNullException(nameof(getDefaultValue))))
        { }

        public Option(
            string[] aliases,
            Func<T> getDefaultValue,
            string? description = null)
            : base(aliases, description, new Argument<T>(getDefaultValue ?? throw new ArgumentNullException(nameof(getDefaultValue))))
        {
        }

        public override IArgumentArity Arity
        {
            get => base.Arity;
            init => Argument.Arity = value;
        }
    }
}