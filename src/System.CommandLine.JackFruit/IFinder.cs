namespace System.CommandLine.JackFruit
{
    public interface IFinder<T>
    {
        T Get<TSource>(Command[] parents, TSource source);
    }
}
