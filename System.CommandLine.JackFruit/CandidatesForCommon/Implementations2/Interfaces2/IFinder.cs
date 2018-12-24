using System.Collections.Generic;

namespace System.CommandLine.JackFruit
{
    public interface IFinder<T>
    {
        T Get<TSource>(TSource source);
        T Get<TSource,TItem>(TSource source, TItem item);
        void AddApproach(Approach<T> approach);
    }

    public interface IListFinder<T>
    {
        IEnumerable<T > Get<TSource>(TSource source);
        void AddApproach(Approach<IEnumerable<T>> approach);
    }
}
