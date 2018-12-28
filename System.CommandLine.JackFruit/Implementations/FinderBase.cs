using System.Collections.Generic;

namespace System.CommandLine.JackFruit
{
    public abstract class FinderBase<TDerived, TReturn> : IFinder<TReturn>
         where TDerived : FinderBase<TDerived, TReturn>
    {
        private ApproachSet<TReturn> approaches;
        private Func<object, object> initialCheck;
        private Func<TReturn, TReturn> finalTransform;

        protected TDerived AddApproach(Approach<TReturn> approach)
        {
            approaches.Add(approach);
            return this as TDerived;
        }

        protected TDerived AddApproachFromFunc<TSource>(Func<Command[], TSource, (bool, TReturn)> approachFunc)
        {
            approaches.Add(Approach<TReturn>.CreateApproach(approachFunc));
            return this as TDerived;
        }

        protected TDerived SetInitialCheck(Func<object, object> initialCheck)
        {
            this.initialCheck = initialCheck;
            return this as TDerived;
        }


        protected TDerived SetFinalTransform(Func<TReturn, TReturn> finalTransform)
        {
            this.finalTransform = finalTransform;
            return this as TDerived;
        }

        protected FinderBase(Func<object, object> initialCheck = null,
                            Func<TReturn, TReturn> finalTransform = null,
                            params Approach<TReturn>[] approaches)
        {
            this.initialCheck = initialCheck;
            this.finalTransform = finalTransform;
            this.approaches = ApproachSet<TReturn>.Create(approaches);
        }


        private protected FinderBase(Func<object, object> initialCheck,
                            Func<TReturn, TReturn> finalTransform,
                            ApproachSet<TReturn> approaches)
        {
            this.initialCheck = initialCheck;
            this.finalTransform = finalTransform;
            this.approaches = approaches;
        }

        public TReturn Get<TSource>(Command[] parents, TSource source)
        {
            if (initialCheck != null)
            {
                source = (TSource)initialCheck(source);
            }
            TReturn ret = approaches.Do(parents, source);
            if (finalTransform != null)
            {
                ret = finalTransform(ret);
            }
            return ret;
        }

    }

    public abstract class FinderBaseForList<TDerived, TReturn> : FinderBase<TDerived, IEnumerable<TReturn>>
        where TDerived : FinderBaseForList<TDerived, TReturn>
    {
        protected FinderBaseForList(Func<object, object> initialCheck = null,
                     Func<IEnumerable<TReturn>, IEnumerable<TReturn>> finalTransform = null,
                     params Approach<IEnumerable<TReturn>>[] approaches)
            : base(initialCheck, finalTransform, ApproachSetForList<TReturn>.Create(approaches))
        { }
    }
}
