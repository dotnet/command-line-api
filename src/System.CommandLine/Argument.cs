// Copyright (c) .NET Foundation and contributors. All rights reserved.
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
    public abstract class Argument : Symbol, IValueDescriptor
    {
        private ArgumentArity _arity;
        private TryConvertArgument? _convertArguments;
        private List<Func<CompletionContext, IEnumerable<CompletionItem>>>? _completions = null;
        private List<Action<ArgumentResult>>? _validators = null;

        /// <summary>
        /// Initializes a new instance of the Argument class.
        /// </summary>
        protected Argument()
        {
        }

        /// <summary>
        /// Initializes a new instance of the Argument class.
        /// </summary>
        /// <param name="name">The name of the argument.</param>
        /// <param name="description">The description of the argument, shown in help.</param>
        protected Argument(string? name = null, string? description = null)
        {
            Name = name!;
            Description = description;
        }

        internal HashSet<string>? AllowedValues { get; private set; }

        /// <summary>
        /// Gets or sets the arity of the argument.
        /// </summary>
        public ArgumentArity Arity
        {
            get
            {
                if (!_arity.IsNonDefault)
                {
                    _arity = ArgumentArity.Default(
                        ValueType, 
                        this, 
                        FirstParent);
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
            init => _convertArguments = value;
        }

        /// <summary>
        /// Gets the collection of completion sources for the argument.
        /// </summary>
        public ICollection<Func<CompletionContext, IEnumerable<CompletionItem>>> Completions =>
            _completions ??= new ()
            {
                CompletionSource.ForType(ValueType)
            };

        /// <summary>
        /// Gets or sets the <see cref="Type" /> that the argument token(s) will be converted to.
        /// </summary>
        public abstract Type ValueType { get; }

        private protected override string DefaultName
        {
            get
            {
                if (FirstParent is not null && FirstParent.Next is null)
                {
                    switch (FirstParent.Symbol)
                    {
                        case Option option:
                            return option.Name;
                        case Command _:
                            return ValueType.Name.ToLowerInvariant();
                    }
                }

                return "";
            }
        }

        internal List<Action<ArgumentResult>> Validators => _validators ??= new ();

        /// <summary>
        /// Gets the default value for the argument.
        /// </summary>
        /// <returns>Returns the default value for the argument, if defined. Null otherwise.</returns>
        public object? GetDefaultValue()
        {
            return GetDefaultValue(new ArgumentResult(this, null));
        }

        internal abstract object? GetDefaultValue(ArgumentResult argumentResult);

        /// <summary>
        /// Specifies if a default value is defined for the argument.
        /// </summary>
        public abstract bool HasDefaultValue { get; }

        internal virtual bool HasCustomParser => false;

        internal void AddAllowedValues(IReadOnlyList<string> values)
        {
            if (AllowedValues is null)
            {
                AllowedValues = new HashSet<string>();
            }

            AllowedValues.UnionWith(values);
        }

        /// <inheritdoc />
        public override IEnumerable<CompletionItem> GetCompletions(CompletionContext context)
        {
            return Completions
                   .SelectMany(source => source.Invoke(context))
                   .Distinct()
                   .OrderBy(c => c.SortText, StringComparer.OrdinalIgnoreCase);
        }

        /// <inheritdoc />
        public override string ToString() => $"{nameof(Argument)}: {Name}";

        /// <inheritdoc />
        string IValueDescriptor.ValueName => Name;

        /// <summary>
        /// Parses a command line string value using the argument.
        /// </summary>
        /// <remarks>The command line string input will be split into tokens as if it had been passed on the command line.</remarks>
        /// <param name="commandLine">A command line string to parse, which can include spaces and quotes equivalent to what can be entered into a terminal.</param>
        /// <returns>A parse result describing the outcome of the parse operation.</returns>
        public ParseResult Parse(string commandLine) =>
            this.GetOrCreateDefaultSimpleParser().Parse(commandLine);

        /// <summary>
        /// Parses a command line string value using the argument.
        /// </summary>
        /// <param name="args">The string arguments to parse.</param>
        /// <returns>A parse result describing the outcome of the parse operation.</returns>
        public ParseResult Parse(string[] args) =>
            this.GetOrCreateDefaultSimpleParser().Parse(args);
    }
}
