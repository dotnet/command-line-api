// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace System.CommandLine
{
    /// <summary>
    /// A collection of directives parsed from a command line.
    /// </summary>
    /// <remarks>A directive is specified on the command line using square brackets, containing no spaces and preceding other tokens unless they are also directives. In the following example, two directives are present, <c>directive-one</c> and <c>directive-two</c>:
    /// <code>    > myapp [directive-one] [directive-two:value] arg1 arg2</code>
    /// The second has a value specified as well, <c>value</c>. Directive values can be read by calling using <see cref="TryGetValues"/>.
    /// </remarks>
    public class DirectiveCollection  : IEnumerable<KeyValuePair<string, IEnumerable<string>>>
    {
        private Dictionary<string, List<string>>? _directives;

        internal void Add(string name, string? value)
        {
            _directives ??= new();

            if (!_directives.TryGetValue(name, out var values))
            {
                values = new List<string>();

                _directives.Add(name, values);
            }

            if (value is not null)
            {
                values.Add(value);
            }
        }

        /// <summary>
        /// Gets a value determining whether a directive with the specified name was parsed.
        /// </summary>
        /// <param name="name">The name of the directive.</param>
        /// <returns><see langword="true"/> if a directive with the specified name was parsed; otherwise, <see langword="false"/>.</returns>
        public bool Contains(string name)
        {
            return _directives is not null && _directives.ContainsKey(name);
        }

        /// <summary>
        /// Gets the values specified for a given directive. A return value indicates whether the specified directive name was present.
        /// </summary>
        /// <param name="name">The name of the directive.</param>
        /// <param name="values">The values provided for the specified directive.</param>
        /// <returns><see langword="true"/> if a directive with the specified name was parsed; otherwise, <see langword="false"/>.</returns>
        public bool TryGetValues(string name, [NotNullWhen(true)] out IReadOnlyList<string>? values)
        {
            if (_directives is not null &&
                _directives.TryGetValue(name, out var v))
            {
                values = v;
                return true;
            }
            else
            {
                values = null;
                return false;
            }
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, IEnumerable<string>>> GetEnumerator()
        {
            if (_directives is null)
            {
                return Enumerable.Empty<KeyValuePair<string, IEnumerable<string>>>().GetEnumerator();
            }
            
            return _directives
                   .Select(pair => new KeyValuePair<string, IEnumerable<string>>(pair.Key, pair.Value))
                   .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
