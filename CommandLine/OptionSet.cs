using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public class OptionSet<T> :
        IReadOnlyCollection<T>
        where T : IAliased
    {
        private readonly HashSet<T> options = new HashSet<T>();

        public OptionSet()
        {
        }

        public OptionSet(IReadOnlyCollection<Option> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            foreach (var option in this.options)
            {
                Add(option);
            }
        }

        public int Count => options.Count;

        public T this[string alias] => options.Single(o => o.HasAlias(alias));

        internal void AddRange(IEnumerable<T> options)
        {
            foreach (var option in options)
            {
                Add(option);
            }
        }

        internal void TryAdd(T option)
        {
            if (!ContainsOptionWithAnyAliasOf(option))
            {
                options.Add(option);
            }
        }

        internal bool ContainsOptionWithAnyAliasOf(T option) =>
            option.Aliases
                  .Any(alias => options
                           .Any(s => s.HasAlias(alias)));

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<T> GetEnumerator() => options.GetEnumerator();

        internal void Add(T option)
        {
            foreach (var alias in option.Aliases)
            {
                if (options.Any(s => s.HasAlias(alias)))
                {
                    throw new ArgumentException($"Alias '{alias}' is already in use.");
                }
            }

            options.Add(option);
        }

        public bool Contains(string alias) => options.Any(s => s.HasAlias(alias));
    }
}