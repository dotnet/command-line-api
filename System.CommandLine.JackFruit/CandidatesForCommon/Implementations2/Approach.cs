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

        internal (bool endEvaluation, TProduce value) Do(Command parent, object source, object item)
            => approachInternal.Do(parent, item, source);

        internal (bool endEvaluation, TProduce value) Do(Command parent, object source)
            => approachInternal.Do(parent, objSource: source);
    }

    public class ApproachSet<TProduce>
    {
        private List<Approach<TProduce>> approaches;
        private bool shortCircuit;

        public static ApproachSet<TProduce> Create(IEnumerable<Approach<TProduce>> approaches, bool? shortCircuit = null)
        {
            if (!shortCircuit.HasValue)
            {
                shortCircuit = !typeof(IEnumerable).IsAssignableFrom(typeof(TProduce));
            }
            return new ApproachSet<TProduce>(approaches, shortCircuit.Value);
        }

        private ApproachSet(IEnumerable<Approach<TProduce>> approaches, bool shortCircuit = true)
        {
            this.approaches = approaches.ToList();
            this.shortCircuit = shortCircuit;
        }

        public void Add(Approach<TProduce> approach)
            => approaches.Add(approach);

        public TProduce Do(Command parent, object objSource, object objItem)
        {
            // TODO: Does try go around for each or around evaluation?
            bool handled = false;
            TProduce value = default;
            foreach (var approach in approaches)
            {
                (handled, value) = approach.Do(parent, objSource, objItem);
                // TODO: value can be a value type, in which case the following may not be right
                if (handled || (shortCircuit && value != default))
                {
                    break;
                }
            }
            return value;
        }


        public TProduce Do(Command parent, object objSource)
        {
            // TODO: Does try go around for each or around evaluation?
            bool handled = false;
            TProduce value = default;
            foreach (var approach in approaches)
            {
                (handled, value) = approach.Do(parent, objSource);
                // TODO: value can be a value type, in which case the following may not be right
                if (handled || (shortCircuit && value != default))
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
