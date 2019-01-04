namespace System.CommandLine.JackFruit
{
    public interface IFinder<T>
    {
        IFinder<T> AddStrategy<TSource>(Func<Command[], TSource, (bool, T)> getFunc);
        T Get<TSource>(Command[] parents, TSource source);
    }
}
