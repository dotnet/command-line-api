using System.Collections.Generic;

namespace System.CommandLine.JackFruit
{
    public interface IFinder<T>
    {
        T Get<TSource>(object parent, TSource source);
        T Get<TSource,TItem>(object parent, TSource source, TItem item);
        void AddApproach(Approach<T> approach);
    }
}
