namespace System.CommandLine.JackFruit
{
    public interface IProvider<T>
    {
        IProvider<T> AddStrategy<TSource>(Func<Command, TSource, (bool, T)> getFunc);
        ProviderBase<T> SetInitialCheck(Func<object, object> initialCheck);
        ProviderBase<T> SetFinalTransform(Func<T, T> finalTransform);
        T Get<TSource>(Command parent, TSource source);
    }
}
