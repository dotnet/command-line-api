﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine.Parsing;
using System.CommandLine.Completions;
using System.Linq;

namespace System.CommandLine
{
    /// <summary>
    /// A symbol defining a value that can be passed on the command line to a <see cref="Command">command</see> or <see cref="Option">option</see>.
    /// </summary>
    public abstract class Argument : Symbol
    {
        private ArgumentArity _arity;
        private TryConvertArgument? _convertArguments;
        private List<Func<CompletionContext, IEnumerable<CompletionItem>>>? _completionSources = null;
        private List<Action<ArgumentResult>>? _validators = null;

        private protected Argument(string name) : base(name, allowWhitespace: true)
        {
        }

        /// <summary>
        /// Gets or sets the arity of the argument.
        /// </summary>
        public ArgumentArity Arity
        {
            get
            {
                if (!_arity.IsNonDefault)
                {
                    _arity = ArgumentArity.Default(this, FirstParent);
                }

                return _arity;
            }
            set => _arity = value;
        }

        /// <summary>
        /// The name used in help output to describe the argument. 
        /// </summary>
        public string? HelpName { get; set; }

        internal TryConvertArgument? ConvertArguments
        {
            get => _convertArguments ??= ArgumentConverter.GetConverter(this);
            set => _convertArguments = value;
        }

        /// <summary>
        /// Gets the list of completion sources for the argument.
        /// </summary>
        public List<Func<CompletionContext, IEnumerable<CompletionItem>>> CompletionSources =>
            _completionSources ??= new ()
            {
                CompletionSource.ForType(ValueType)
            };

        /// <summary>
        /// Gets or sets the <see cref="Type" /> that the argument token(s) will be converted to.
        /// </summary>
        public abstract Type ValueType { get; }

        /// <summary>
        /// Provides a list of argument validators. Validators can be used
        /// to provide custom errors based on user input.
        /// </summary>
        public List<Action<ArgumentResult>> Validators => _validators ??= new ();

        internal bool HasValidators => (_validators?.Count ?? 0) > 0;

        /// <summary>
        /// Gets the default value for the argument.
        /// </summary>
        /// <returns>Returns the default value for the argument, if defined. Null otherwise.</returns>
        public object? GetDefaultValue()
        {
            return GetDefaultValue(new ArgumentResult(this, null!, null));
        }

        internal abstract object? GetDefaultValue(ArgumentResult argumentResult);

        /// <summary>
        /// Specifies if a default value is defined for the argument.
        /// </summary>
        public abstract bool HasDefaultValue { get; }

        /// <inheritdoc />
        public override IEnumerable<CompletionItem> GetCompletions(CompletionContext context)
        {
            return CompletionSources
                   .SelectMany(source => source.Invoke(context))
                   .Distinct()
                   .OrderBy(c => c.SortText, StringComparer.OrdinalIgnoreCase);
        }

        /// <inheritdoc />
        public override string ToString() => $"{nameof(Argument)}: {Name}";

        internal bool IsBoolean() => ValueType == typeof(bool) || ValueType == typeof(bool?);
    }
}
