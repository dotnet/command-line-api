// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.CommandLine.Parsing;

namespace System.CommandLine
{
    /// <inheritdoc cref="Option" />
    /// <typeparam name="T">The <see cref="System.Type"/> that the option's arguments are expected to be parsed as.</typeparam>
    public class Option<T> : Option, IValueDescriptor<T>
    {
        /// <inheritdoc/>
        public Option(
            string name,
            string? description = null) 
            : base(name, description, new Argument<T>())
        { }

        /// <inheritdoc/>
        public Option(
            string[] aliases,
            string? description = null) 
            : base(aliases, description, new Argument<T>())
        { }

        /// <inheritdoc/>
        public Option(
            string name,
            ParseArgument<T> parseArgument,
            bool isDefault = false,
            string? description = null) 
            : base(name, description, 
                  new Argument<T>(parseArgument ?? throw new ArgumentNullException(nameof(parseArgument)), isDefault))
        { }

        /// <inheritdoc/>
        public Option(
            string[] aliases,
            ParseArgument<T> parseArgument,
            bool isDefault = false,
            string? description = null) 
            : base(aliases, description, new Argument<T>(parseArgument ?? throw new ArgumentNullException(nameof(parseArgument)), isDefault))
        { }

        /// <inheritdoc/>
        public Option(
            string name,
            Func<T> getDefaultValue,
            string? description = null) 
            : base(name, description, 
                  new Argument<T>(getDefaultValue ?? throw new ArgumentNullException(nameof(getDefaultValue))))
        { }

        /// <inheritdoc/>
        public Option(
            string[] aliases,
            Func<T> getDefaultValue,
            string? description = null)
            : base(aliases, description, new Argument<T>(getDefaultValue ?? throw new ArgumentNullException(nameof(getDefaultValue))))
        {
        }

        /// <inheritdoc/>
        public override ArgumentArity Arity
        {
            get => base.Arity;
            set => Argument.Arity = value;
        }
    }
}