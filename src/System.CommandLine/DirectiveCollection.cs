// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace System.CommandLine
{
    internal class DirectiveCollection : IDirectiveCollection
    {
        private readonly Dictionary<string, List<string>> _directives = new();

        public void Add(string name, string? value)
        {
            if (!_directives.TryGetValue(name, out var values))
            {
                values = new List<string>();

                _directives.Add(name, values);
            }

            if (value != null)
            {
                values.Add(value);
            }
        }

        public bool Contains(string name)
        {
            return _directives.ContainsKey(name);
        }

        public bool TryGetValues(string name,  [NotNullWhen(true)] out IEnumerable<string>? values)
        {
            if (_directives.TryGetValue(name, out var v))
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

        public IEnumerator<KeyValuePair<string, IEnumerable<string>>> GetEnumerator() =>
            _directives
                .Select(pair => new KeyValuePair<string, IEnumerable<string>>(pair.Key, pair.Value))
                .GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
