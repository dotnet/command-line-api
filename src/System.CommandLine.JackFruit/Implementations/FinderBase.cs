using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine.JackFruit
{
    public class FinderBase 
    {
        public static FinderBase<T> Create<T>()
        {
            if (typeof(T) != typeof(string) && typeof(IEnumerable).IsAssignableFrom(typeof(T)))
            {
                var itemType = typeof(T).GetGenericArguments().First();
                var typeDef = typeof(StrategySetForList<>);
                var newType = typeDef.MakeGenericType(new Type[] { itemType });
                var strategySet = Activator.CreateInstance(newType, true);
                return new FinderBase<T>(strategies: strategySet);
            }
            return new FinderBase<T>(strategies: new StrategySet<T>());
        }
    }

    public class FinderBase<TReturn> :FinderBase, IFinder<TReturn>
    {
        private StrategySet<TReturn> strategies;
        private Func<object, object> initialCheck;
        private Func<TReturn, TReturn> finalTransform;

   
        internal FinderBase(object strategies)
            => this.strategies = (StrategySet<TReturn>)strategies;

        internal FinderBase(StrategySet<TReturn> strategies)
            => this.strategies = strategies;

        public IFinder<TReturn> AddStrategy<TSource>(Func<Command[], TSource, (bool, TReturn)> strategy)
        {
            strategies.Add(Strategy<TReturn>.CreateStrategy(strategy));
            return this as FinderBase<TReturn>;
        }

        public FinderBase<TReturn> SetInitialCheck(Func<object, object> initialCheck)
        {
            this.initialCheck = initialCheck;
            return this as FinderBase<TReturn>;
        }

        public FinderBase<TReturn> SetFinalTransform(Func<TReturn, TReturn> finalTransform)
        {
            this.finalTransform = finalTransform;
            return this as FinderBase<TReturn>;
        }

        public TReturn Get<TSource>(Command[] parents, TSource source)
        {
            if (initialCheck != null)
            {
                source = (TSource)initialCheck(source);
            }
            var ret = strategies.Do(parents, source);
            if (finalTransform != null)
            {
                ret = finalTransform(ret);
            }
            return ret;
        }
    }
}
