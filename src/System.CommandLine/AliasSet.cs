using System.Collections;
using System.Collections.Generic;

namespace System.CommandLine
{
    // this types exists only because we need to validate the added aliases ;)
    internal sealed class AliasSet : ICollection<string>
    {
        private readonly HashSet<string> _aliases;

        internal AliasSet(bool caseSensitive) => _aliases = new(caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);

        internal AliasSet(string[] aliases, bool caseSensitive)
        {
            foreach (string alias in aliases)
            {
                CliSymbol.ThrowIfEmptyOrWithWhitespaces(alias, nameof(alias));
            }

            _aliases = new(aliases, caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);
        }

        public int Count => _aliases.Count;

        public bool IsReadOnly => false;

        public void Add(string item)
            => _aliases.Add(CliSymbol.ThrowIfEmptyOrWithWhitespaces(item, nameof(item)));

        internal bool Overlaps(AliasSet other) => _aliases.Overlaps(other._aliases);


        // a struct based enumerator for avoiding allocations
        public HashSet<string>.Enumerator GetEnumerator() => _aliases.GetEnumerator();

        public void Clear() => _aliases.Clear();

        public bool Contains(string item) => _aliases.Contains(item);

        public void CopyTo(string[] array, int arrayIndex) => _aliases.CopyTo(array, arrayIndex);

        public bool Remove(string item) => _aliases.Remove(item);

        IEnumerator<string> IEnumerable<string>.GetEnumerator() => _aliases.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _aliases.GetEnumerator();
    }
}
