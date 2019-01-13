using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine.JackFruit
{
    public interface IStrategy<TProduce>
    {
        TProduce Do(Command parent, object source);
    }

    public class Strategy<TProduce>
    {
        public static Strategy<TProduce, TSource> CreateStrategy<TSource>(
                  Func<Command, TSource, TProduce> operation)
              => new Strategy<TProduce, TSource>(operation);
    }

    public class Strategy<TProduce, TSource> : IStrategy<TProduce>
    {
        private Func<Command, TSource, TProduce> operation;

        internal Strategy(Func<Command, TSource, TProduce> operation)
            => this.operation = operation;

        // If the object is a different type, just do nothing. 
        TProduce IStrategy<TProduce>.Do(Command parent, object objSource)
        => (operation != null && objSource is TSource source)
                ? operation(parent, source)
                : default;
    }

    internal class StrategySet<TProduce>
    {
        protected readonly List<IStrategy<TProduce>> strategies;
        protected readonly bool shortCircuit;

        internal StrategySet()
        {
            strategies = new List<IStrategy<TProduce>>();
            shortCircuit = !typeof(Enumerable).IsAssignableFrom(typeof(TProduce));
        }

        public void Add(IStrategy<TProduce> strategy)
            => strategies.Add(strategy);

        public TProduce Do(Command parent, object objSource)
            => DoInternal(a => a.Do(parent, objSource));

        protected virtual TProduce DoInternal(Func<IStrategy<TProduce>, TProduce> operation)
        {
            // TODO: Does try go around for each or around evaluation?
            bool endEvaluation = false;
            TProduce value = default;
            foreach (var strategy in strategies)
            {

                value = operation(strategy);
                if (endEvaluation || shortCircuit &&
                    (value is string s
                       ? !string.IsNullOrWhiteSpace(s)
                       : value != default))
                {
                    break;
                }
            }
            return value;
        }
    }

    // This class exists because lists have a differnt algorithm for Do and need extra information <T>
    internal class StrategySetForList<T> : StrategySet<IEnumerable<T>>
    {
        internal StrategySetForList()
            : base() { }

        // TProduce is IEnumerable<T>
        protected override IEnumerable<T> DoInternal(Func<IStrategy<IEnumerable<T>>, IEnumerable<T>> operation)
        {
            var value = new List<T>();
            foreach (var strategy in strategies)
            {
                var newList = operation(strategy);
                if (newList == null)
                {
                    continue;
                }
                if (newList.Any())
                {
                    value.AddRange(newList);
                }
                if (shortCircuit && newList.Any())
                {
                    break;
                }
            }
            return value;
        }
    }
}
