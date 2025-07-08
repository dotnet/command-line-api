// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;

namespace System.CommandLine
{
    /// <inheritdoc cref="Option" />
    /// <typeparam name="T">The <see cref="System.Type"/> that the option's arguments are expected to be parsed as.</typeparam>
    public class Option<T> : Option
    {
        internal readonly Argument<T> _argument;

        /// <summary>
        /// Initializes a new instance of the <see cref="Option"/> class.
        /// </summary>
        /// <param name="name">The name of the option. This is used during parsing and is displayed in help.</param>
        /// <param name="aliases">Optional aliases by which the option can be specified on the command line.</param>
        public Option(string name, params string[] aliases) 
            : this(name, aliases, new Argument<T>(name))
        {
        }

        private protected Option(string name, string[] aliases, Argument<T> argument)
            : base(name, aliases)
        {
            argument.AddParent(this);
            _argument = argument;
        }

        /// <inheritdoc cref="Argument{T}.DefaultValueFactory" />
        public Func<ArgumentResult, T>? DefaultValueFactory
        {
            get => _argument.DefaultValueFactory;
            set => _argument.DefaultValueFactory = value;
        }

        /// <inheritdoc cref="Argument{T}.CustomParser" />
        public Func<ArgumentResult, T?>? CustomParser
        {
            get => _argument.CustomParser;
            set => _argument.CustomParser = value;
        }
        
        internal sealed override Argument Argument => _argument;

        /// <inheritdoc />
        public override Type ValueType => _argument.ValueType;

        /// <summary>
        /// Configures the option to accept only the specified values, and to suggest them as command line completions.
        /// </summary>
        /// <param name="values">The values that are allowed for the option.</param>
        public Option<T> AcceptOnlyFromAmong(params string[] values)
        {
            _argument.AcceptOnlyFromAmong(values);
            return this;
        }

        /// <summary>
        /// Configures the option to accept only values representing legal file paths.
        /// </summary>
        public Option<T> AcceptLegalFilePathsOnly()
        {
            _argument.AcceptLegalFilePathsOnly();
            return this;
        }

        /// <summary>
        /// Configures the option to accept only values representing legal file names.
        /// </summary>
        /// <remarks>A parse error will result, for example, if file path separators are found in the parsed value.</remarks>
        public Option<T> AcceptLegalFileNamesOnly()
        {
            _argument.AcceptLegalFileNamesOnly();
            return this;
        }
    }
}