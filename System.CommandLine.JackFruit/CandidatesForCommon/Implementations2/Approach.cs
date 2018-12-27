using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.CommandLine.JackFruit
{
    public class Approach<TProduce>
    {
        private ApproachInternal approachInternal;

        private Approach(ApproachInternal approachInternal)
        {
            this.approachInternal = approachInternal;
        }

        private abstract class ApproachInternal
        {
            // TODO: Consider private bool failFast = false;
            internal abstract (bool endEvaluation, TProduce) Do(Command parent, object objSource, object objItem);
            internal abstract (bool endEvaluation, TProduce) Do(Command parent, object objSource);
        }

        private class ApproachInternal<TSource> : ApproachInternal
        {
            private Func<Command, TSource, (bool, TProduce)> operationWithoutItem;

            internal ApproachInternal(Func<Command, TSource, (bool, TProduce)> operation)
                => this.operationWithoutItem = operation;

            internal override (bool endEvaluation, TProduce) Do(Command parent, object objSource)
                 => (operationWithoutItem != null && objSource is TSource source)
                    ? operationWithoutItem(parent, source)
                    : (false, default);

            internal override (bool endEvaluation, TProduce) Do(Command parent, object objSource, object objItem)
                => throw new InvalidOperationException();
        }

        private class ApproachInternal<TSource, TItem> : ApproachInternal<TSource>
        {
            private Func<Command, TSource, TItem, (bool, TProduce)> operation;

            internal ApproachInternal(Func<Command, TSource, TItem, (bool, TProduce)> operation,
                        Func<Command, TSource, (bool, TProduce)> operationWithoutItem = null)
                : base(operationWithoutItem)
                => this.operation = operation;

            internal override (bool endEvaluation, TProduce) Do(Command parent, object objSource, object objItem)
            {
                if (operation != null && objSource is TSource source)
                {
                    if (objItem is TItem item)
                    {
                        return operation(parent, source, item);
                    }
                    return operation(parent, source, default);
                }
                throw new InvalidOperationException();
            }
        }

        public static Approach<TProduce> CreateApproach<TSource, TItem>(
                Func<Command, TSource, TItem, (bool, TProduce)> operation,
                Func<Command, TSource, (bool, TProduce)> operationWithoutItem = null)
            => new Approach<TProduce>(
                new Approach<TProduce>.ApproachInternal<TSource, TItem>(operation, operationWithoutItem));

        public static Approach<TProduce> CreateApproach<TSource>(
                Func<Command, TSource, (bool, TProduce)> operation)
            => new Approach<TProduce>(
                new Approach<TProduce>.ApproachInternal<TSource>(operation));

        internal (bool EndEvaluation, TProduce Value) Do(Command parent, object source, object item)
            => approachInternal.Do(parent, item, source);

        internal (bool EndEvaluation, TProduce Value) Do(Command parent, object source)
            => approachInternal.Do(parent, objSource: source);
    }

    internal class ApproachSet<TProduce>
    {
        protected readonly List<Approach<TProduce>> approaches;
        protected readonly bool shortCircuit;

        public static ApproachSet<TProduce> Create(IEnumerable<Approach<TProduce>> approaches, bool shortCircuit = true)
        {
            return new ApproachSet<TProduce>(approaches, shortCircuit);
        }

        protected ApproachSet(IEnumerable<Approach<TProduce>> approaches, bool shortCircuit = true)
        {
            this.approaches = approaches.ToList();
            this.shortCircuit = shortCircuit;
        }

        public void Add(Approach<TProduce> approach)
            => approaches.Add(approach);

        public TProduce Do(Command parent, object objSource, object objItem)
        {
            return DoInternal(a => a.Do(parent, objSource, objItem));
        }

        public TProduce Do(Command parent, object objSource)
        {
            return DoInternal( a => a.Do(parent, objSource));
        }

        protected virtual TProduce DoInternal( Func<Approach<TProduce>,( bool, TProduce)> operation)
        {
            bool endEvaluation = false;
            TProduce value = default;
            foreach (var approach in approaches)
            {

                (endEvaluation, value) = operation(approach);
                if (endEvaluation || shortCircuit && value != default)
                {
                    break;
                }
            }
            return value;
        }
    }

    internal class ApproachSetForList<T> : ApproachSet<IEnumerable<T>>
    {

        public new static ApproachSet<IEnumerable<T>> Create(IEnumerable<Approach<IEnumerable<T>>> approaches, bool shortCircuit = false)
        {
            return new ApproachSetForList<T>(approaches, shortCircuit);
        }

        private ApproachSetForList(IEnumerable<Approach<IEnumerable<T>>> approaches, bool shortCircuit = true)
            :base(approaches, shortCircuit)
        {  }

        protected override IEnumerable<T> DoInternal(Func<Approach<IEnumerable<T>>, (bool, IEnumerable<T>)> operation)
        {
            // TODO: Does try go around for each or around evaluation?
            var value = new List<T>();
            foreach (var approach in approaches)
            {
                (var endEvaluation, var newList) = operation(approach);
                if (newList != null && newList.Any())
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

    //public class ApproachSetForList<T> : ApproachSet<IEnumerable<T>>
    //{
    //    public ApproachSetForList(IEnumerable<Approach<IEnumerable<T>>> approaches, bool shortCircuit = false)
    //        : base(approaches, shortCircuit)
    //    { }
    //}
}
