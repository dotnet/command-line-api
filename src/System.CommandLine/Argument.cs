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
    public abstract class Argument : Symbol
    {
        private ArgumentArity _arity;
        private TryConvertArgument? _convertArguments;
        private List<Func<CompletionContext, IEnumerable<CompletionItem>>>? _completionSources = null;
        private List<Action<ArgumentResult>>? _validators = null;

        /// <summary>
        /// Initializes a new instance of the Argument class.
        /// </summary>
        /// <param name="name">The name of the argument. This can be used to look up the parsed value and is displayed in help</param>
        protected Argument(string name) : base(name, allowWhitespace: true)
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
        public List<Func<CompletionContext, IEnumerable<CompletionItem>>> CompletionSources
        {
            get
            {
                if (_completionSources is null)
                {
                    Type? valueType = ValueType;
                    if (IsBoolean())
                    {
                        _completionSources = new ()
                        {
                            static _ => new CompletionItem[]
                            {
                                new(bool.TrueString),
                                new(bool.FalseString)
                            }
                        };
                    }
                    else if (!valueType.IsPrimitive && (valueType.IsEnum || (valueType.TryGetNullableType(out valueType) && valueType.IsEnum)))
                    {
                        _completionSources = new()
                        {
                            _ => Enum.GetNames(valueType).Select(n => new CompletionItem(n))
                        };
                    }
                    else
                    {
                        _completionSources = new();
                    }
                }

                return _completionSources;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Type" /> that the argument's parsed tokens will be converted to.
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

        internal static Argument None { get; } = new NoArgument();

        private sealed class NoArgument : Argument
        {
            internal NoArgument() : base("@none")
            {
            }

            public override Type ValueType { get; } = typeof(void);

            internal override object? GetDefaultValue(ArgumentResult argumentResult) => null;

            public override bool HasDefaultValue => false;
        }
    }
}
