namespace System.CommandLine.JackFruit
{
    public interface IStrategySet<T>
    {
        IStrategySet<T> AddStrategy<TSource>(Func<Command, TSource,  T> getFunc);
        StrategySetBase<T> SetInitialCheck(Func<object, object> initialCheck);
        StrategySetBase<T> SetFinalTransform(Func<T, T> finalTransform);
        T Get<TSource>(Command parent, TSource source);
    }
}
