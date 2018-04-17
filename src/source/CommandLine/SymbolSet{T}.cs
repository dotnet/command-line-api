// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Microsoft.DotNet.Cli.CommandLine
{
    [DebuggerStepThrough]
    public abstract class SymbolSet<T> : IReadOnlyCollection<T>
        where T : class
    {
        private readonly HashSet<T> options = new HashSet<T>();

        protected SymbolSet()
        {
        }

        protected SymbolSet(IReadOnlyCollection<T> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            foreach (var option in options)
            {
                Add(option);
            }
        }

        public T this[string alias] =>
            options.SingleOrDefault(o => ContainsItemWithRawAlias(o, alias)) ??
            options.SingleOrDefault(o => ContainsItemWithAlias(o, alias));

        public int Count => options.Count;

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return options.GetEnumerator();
        }

        internal void AddRange(IEnumerable<T> options)
        {
            foreach (var option in options)
            {
                Add(option);
            }
        }

        protected abstract bool ContainsItemWithAlias(T option, string alias);

        protected abstract bool ContainsItemWithRawAlias(T option, string alias);

        internal void Add(T option)
        {
            var preexistingAlias = RawAliasesFor(option)
                .FirstOrDefault(alias =>
                                    options.Any(o =>
                                                    ContainsItemWithRawAlias(o, alias)));

            if (preexistingAlias != null)
            {
                throw new ArgumentException($"Alias '{preexistingAlias}' is already in use.");
            }

            options.Add(option);
        }

        protected abstract IReadOnlyCollection<string> RawAliasesFor(T option);

        public bool Contains(string alias) => 
            options.Any(option => ContainsItemWithAlias(option, alias));
    }
}