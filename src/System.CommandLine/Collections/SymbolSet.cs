// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;

namespace System.CommandLine.Collections
{
    /// <summary>
    /// A set of symbols, unique and indexed by their aliases.
    /// </summary>
    public class SymbolSet : IEnumerable<Symbol>
    {
        private readonly List<Symbol> _symbols = new List<Symbol>();
        private List<Argument>? _arguments;
        private List<Option>? _options;

        public int Count => _symbols.Count;

        public Symbol this[int index] => _symbols[index];

        private void ResetIndex(ISymbol item)
        {
            switch (item)
            {
                case Argument _:
                    _arguments = null;
                    break;
                case Option _:
                    _options = null;
                    break;
            }
        }

        internal void AddWithoutAliasCollisionCheck(Symbol item)
        {
            _symbols.Add(item);

            if (_arguments is not null || _options is not null)
            {
                ResetIndex(item);
            }
        }

        internal bool IsAnyAliasInUse(Option option)
        {
            if (_symbols.Count > 0)
            {
                string name = option.Name;
                foreach (Symbol item in _symbols)
                {
                    if (item.Matches(name))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public IEnumerator<Symbol> GetEnumerator() => _symbols.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _symbols.GetEnumerator();

        internal IReadOnlyList<Argument> Arguments
        {
            get
            {
                return _arguments ??= BuildArgumentsList();

                List<Argument> BuildArgumentsList()
                {
                    var arguments = new List<Argument>(_symbols.Count);

                    for (var i = 0; i < _symbols.Count; i++)
                    {
                        if (_symbols[i] is Argument argument)
                        {
                            arguments.Add(argument);
                        }
                    }

                    return arguments;
                }
            }
        }
        
        internal IReadOnlyList<Option> Options
        {
            get
            {
                return _options ??= BuildOptionsList();

                List<Option> BuildOptionsList()
                {
                    var options = new List<Option>(_symbols.Count);

                    for (var i = 0; i < _symbols.Count; i++)
                    {
                        if (_symbols[i] is Option Option)
                        {
                            options.Add(Option);
                        }
                    }

                    return options;
                }
            }
        }
    }
}
