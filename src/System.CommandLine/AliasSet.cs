using System.Collections;
using System.Collections.Generic;

namespace System.CommandLine
{
    // this types exists only because we need to validate the added aliases ;)
    // TODO: add struct enumerator
    internal sealed class AliasSet : ICollection<string>
    {
        private readonly HashSet<string> _aliases;

        internal AliasSet() => _aliases = new(StringComparer.Ordinal);

        internal AliasSet(string[] aliases)
        {
            foreach (string alias in aliases)
            {
                Symbol.ThrowIfEmptyOrWithWhitespaces(alias, nameof(alias));
            }

            _aliases = new(aliases, StringComparer.Ordinal);
        }

        public int Count => _aliases.Count;

        public bool IsReadOnly => throw new NotImplementedException();

        public void Add(string item)
            => _aliases.Add(Symbol.ThrowIfEmptyOrWithWhitespaces(item, nameof(item)));

        internal bool Overlaps(AliasSet other) => _aliases.Overlaps(other._aliases);

        public void Clear() => _aliases.Clear();

        public bool Contains(string item) => _aliases.Contains(item);

        public void CopyTo(string[] array, int arrayIndex) => _aliases.CopyTo(array, arrayIndex);

        public IEnumerator<string> GetEnumerator() => _aliases.GetEnumerator();

        public bool Remove(string item) => _aliases.Remove(item);

        IEnumerator IEnumerable.GetEnumerator() => _aliases.GetEnumerator();
    }
}
