using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.CommandLine.JackFruit
{
    public class Approach<T>
    {
        private ApproachInternal approachInternal;

        private Approach(ApproachInternal approachInternal)
        {
            this.approachInternal = approachInternal;
        }

        private abstract class ApproachInternal
        {
            // TODO: Consider private bool failFast = false;
            internal abstract (bool endEvaluation, T) Do(object objSource, object objItem);
            internal abstract (bool endEvaluation, T) Do(object objSource);
        }

        private class ApproachInternal<TSource> : ApproachInternal
        {
            private Func<TSource, (bool, T)> operationWithoutItem;

            internal ApproachInternal(Func<TSource, (bool, T)> operation)
                => this.operationWithoutItem = operation;

            internal override (bool endEvaluation, T) Do(object objSource)
                 => (operationWithoutItem != null && objSource is TSource source)
                    ? operationWithoutItem(source)
                    : (false, default);

            internal override (bool endEvaluation, T) Do(object objSource, object objItem)
                => throw new InvalidOperationException();
        }

        private class ApproachInternal<TSource, TItem> : ApproachInternal<TSource>
        {
            private Func<TSource, TItem, (bool, T)> operation;

            internal ApproachInternal(Func<TSource, TItem, (bool, T)> operation,
                        Func<TSource, (bool, T)> operationWithoutItem = null)
                : base(operationWithoutItem)
                => this.operation = operation;

            internal override (bool endEvaluation, T) Do(object objSource, object objItem)
            {
                if (operation != null && objSource is TSource source)
                {
                    if (objItem is TItem item)
                    {
                        return operation(source, item);
                    }
                    return operation(source, default);
                }
                throw new InvalidOperationException();
            }
        }

        public static Approach<T> CreateApproach<TSource, TItem>(
                Func<TSource, TItem, (bool, T)> operation,
                Func<TSource, (bool, T)> operationWithoutItem = null)
            => new Approach<T>(
                new Approach<T>.ApproachInternal<TSource, TItem>(operation, operationWithoutItem));

        public static Approach<T> CreateApproach<TSource>(
                Func<TSource, (bool, T)> operation)
            => new Approach<T>(
                new Approach<T>.ApproachInternal<TSource>(operation));

        internal (bool endEvaluation, T value) Do(object source, object item)
            => approachInternal.Do(source, item);

        internal (bool endEvaluation, T value) Do(object source)
            => approachInternal.Do(source);
    }
    public class ApproachSet<T>
    {
        private List<Approach<T>> approaches;
        private bool shortCircuit;

        public ApproachSet(IEnumerable<Approach<T>> approaches, bool shortCircuit = true)
        {
            this.approaches = approaches.ToList();
            this.shortCircuit = shortCircuit;
        }

        public void Add(Approach<T> approach)
            => approaches.Add(approach);

        public T Do(object objSource, object objItem)
        {
            // TODO: Does try go around for each or around evaluation?
            bool handled = false;
            T value = default;
            foreach (var approach in approaches)
            {
                (handled, value) = approach.Do(objSource, objItem);
                // TODO: value can be a value type, in which case the following may not be right
                if (handled || (shortCircuit && value != default))
                {
                    break;
                }
            }
            return value;
        }


        public T Do(object objSource)
        {
            // TODO: Does try go around for each or around evaluation?
            bool handled = false;
            T value = default;
            foreach (var approach in approaches)
            {
                (handled, value) = approach.Do(objSource);
                // TODO: value can be a value type, in which case the following may not be right
                if (handled || (shortCircuit && value != default))
                {
                    break;
                }
            }
            return value;
        }

    }

    public class ApproachSetForList<T> : ApproachSet<IEnumerable<T>>
    {
        public ApproachSetForList(IEnumerable<Approach<IEnumerable<T>>> approaches, bool shortCircuit = false)
            : base(approaches, shortCircuit)
        { }
    }
}
