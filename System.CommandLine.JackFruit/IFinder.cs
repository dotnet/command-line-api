namespace System.CommandLine.JackFruit
{
    public interface IFinder<T>
    {
        T Get<TSource>(Command parent, TSource source);
        T Get<TSource,TItem>(Command parent, TSource source, TItem item);
        void AddApproach(Approach<T> approach);
    }
}
