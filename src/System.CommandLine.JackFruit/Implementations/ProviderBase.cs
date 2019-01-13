using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine.JackFruit
{
    public class ProviderBase 
    {
        public static ProviderBase<T> Create<T>()
        {
            if (typeof(T) != typeof(string) && typeof(IEnumerable).IsAssignableFrom(typeof(T)))
            {
                var itemType = typeof(T).GetGenericArguments().First();
                var typeDef = typeof(StrategySetForList<>);
                var newType = typeDef.MakeGenericType(new Type[] { itemType });
                var strategySet = Activator.CreateInstance(newType, true);
                return new ProviderBase<T>(strategies: strategySet);
            }
            return new ProviderBase<T>(strategies: new StrategySet<T>());
        }
    }

    public class ProviderBase<TReturn> :ProviderBase, IProvider<TReturn>
    {
        private StrategySet<TReturn> strategies;
        private Func<object, object> initialCheck;
        private Func<TReturn, TReturn> finalTransform;

   
        internal ProviderBase(object strategies)
            => this.strategies = (StrategySet<TReturn>)strategies;

        internal ProviderBase(StrategySet<TReturn> strategies)
            => this.strategies = strategies;

        public IProvider<TReturn> AddStrategy<TSource>(Func<Command, TSource, TReturn> strategy)
        {
            strategies.Add(Strategy<TReturn>.CreateStrategy(strategy));
            return this as ProviderBase<TReturn>;
        }

        public ProviderBase<TReturn> SetInitialCheck(Func<object, object> initialCheck)
        {
            this.initialCheck = initialCheck;
            return this as ProviderBase<TReturn>;
        }

        public ProviderBase<TReturn> SetFinalTransform(Func<TReturn, TReturn> finalTransform)
        {
            this.finalTransform = finalTransform;
            return this as ProviderBase<TReturn>;
        }

        public TReturn Get<TSource>(Command parent, TSource source)
        {
            if (initialCheck != null)
            {
                source = (TSource)initialCheck(source);
            }
            var ret = strategies.Do(parent, source);
            if (finalTransform != null)
            {
                ret = finalTransform(ret);
            }
            return ret;
        }
    }
}
