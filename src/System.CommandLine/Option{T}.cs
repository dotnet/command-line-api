// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;

namespace System.CommandLine
{
    /// <inheritdoc cref="CliOption" />
    /// <typeparam name="T">The <see cref="System.Type"/> that the option's arguments are expected to be parsed as.</typeparam>
    public class CliOption<T> : CliOption
    {
        internal readonly CliArgument<T> _argument;

        /// <summary>
        /// Initializes a new instance of the Option class.
        /// </summary>
        /// <param name="name">The name of the option. It's used for parsing, displaying Help and creating parse errors.</param>>
        /// <param name="aliases">Optional aliases. Used for parsing, suggestions and displayed in Help.</param>
        public CliOption(string name, params string[] aliases) 
            : this(name, aliases, new CliArgument<T>(name))
        {
        }

        private protected CliOption(string name, CliArgument<T> argument) : base(name)
        {
            argument.AddParent(this);
            _argument = argument;
        }

        private protected CliOption(string name, string[] aliases, CliArgument<T> argument)
            : base(name, aliases)
        {
            argument.AddParent(this);
            _argument = argument;
        }

        /// <inheritdoc cref="CliArgument{T}.DefaultValueFactory" />
        public Func<ArgumentResult, T>? DefaultValueFactory
        {
            get => _argument.DefaultValueFactory;
            set => _argument.DefaultValueFactory = value;
        }

        /// <inheritdoc cref="CliArgument{T}.CustomParser" />
        public Func<ArgumentResult, T?>? CustomParser
        {
            get => _argument.CustomParser;
            set => _argument.CustomParser = value;
        }

        /// <inheritdoc />
        public override Type ValueType => typeof(T);

        internal sealed override CliArgument Argument => _argument;

        /// <summary>
        /// Configures the option to accept only the specified values, and to suggest them as command line completions.
        /// </summary>
        /// <param name="values">The values that are allowed for the option.</param>
        public void AcceptOnlyFromAmong(params string[] values) => _argument.AcceptOnlyFromAmong(values);

        /// <summary>
        /// Configures the option to accept only values representing legal file paths.
        /// </summary>
        public void AcceptLegalFilePathsOnly() => _argument.AcceptLegalFilePathsOnly();

        /// <summary>
        /// Configures the option to accept only values representing legal file names.
        /// </summary>
        /// <remarks>A parse error will result, for example, if file path separators are found in the parsed value.</remarks>
        public void AcceptLegalFileNamesOnly() => _argument.AcceptLegalFileNamesOnly();
    }
}