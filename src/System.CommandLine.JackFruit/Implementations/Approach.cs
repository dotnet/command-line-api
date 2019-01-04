using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine.JackFruit
{
    public interface IApproach<TProduce>
    {
        (bool EndEvaluation, TProduce Value) Do(Command[] parents, object source);
    }

    public class Approach<TProduce>
    {
        public static Approach<TProduce, TSource> CreateApproach<TSource>(
                  Func<Command[], TSource, (bool, TProduce)> operation)
              => new Approach<TProduce, TSource>(operation);
    }

    public class Approach<TProduce, TSource> : IApproach<TProduce>
    {
        private Func<Command[], TSource, (bool, TProduce)> operation;

        internal Approach(Func<Command[], TSource, (bool, TProduce)> operation)
            => this.operation = operation;

        // If the object is a different type, just do nothing. 
        (bool EndEvaluation, TProduce Value) IApproach<TProduce>.Do(Command[] parents, object objSource)
        => (operation != null && objSource is TSource source)
                ? operation(parents, source)
                : (false, default);
    }

    internal class ApproachSet<TProduce>
    {
        protected readonly List<IApproach<TProduce>> approaches;
        protected readonly bool shortCircuit;

        internal ApproachSet()
        {
            approaches = new List<IApproach<TProduce>>();
            shortCircuit = !typeof(Enumerable).IsAssignableFrom(typeof(TProduce));
        }

        public void Add(IApproach<TProduce> approach)
            => approaches.Add(approach);

        public TProduce Do(Command[] parents, object objSource)
            => DoInternal(a => a.Do(parents, objSource));

        protected virtual TProduce DoInternal(Func<IApproach<TProduce>, (bool, TProduce)> operation)
        {
            // TODO: Does try go around for each or around evaluation?
            bool endEvaluation = false;
            TProduce value = default;
            foreach (var approach in approaches)
            {

                (endEvaluation, value) = operation(approach);
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
    internal class ApproachSetForList<T> : ApproachSet<IEnumerable<T>>
    {
        internal ApproachSetForList()
            : base() { }

        // TProduce is IEnumerable<T>
        protected override IEnumerable<T> DoInternal(Func<IApproach<IEnumerable<T>>, (bool, IEnumerable<T>)> operation)
        {
            var value = new List<T>();
            foreach (var approach in approaches)
            {
                (var endEvaluation, var newList) = operation(approach);
                if (newList == null)
                {
                    continue;
                }
                if (newList.Any())
                {
                    value.AddRange(newList);
                }
                if (endEvaluation || (shortCircuit && newList.Any()))
                {
                    break;
                }
            }
            return value;
        }
    }
}
