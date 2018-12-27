using System.Collections.Generic;

namespace System.CommandLine.JackFruit
{
    public abstract class FinderBase<T> : IFinder<T>
    {
        private ApproachSet<T> approaches;
        private Func<object, object> initialCheck;
        private Func<T, T> finalTransform;

        public void AddApproach(Approach<T> approach)
            => approaches.Add(approach);

        protected FinderBase(Func<object, object> initialCheck = null,
                            Func<T, T> finalTransform = null,
                            params Approach<T>[] approaches)
        {
            this.initialCheck = initialCheck;
            this.finalTransform = finalTransform;
            this.approaches = ApproachSet<T>.Create(approaches);
        }


        private protected FinderBase(Func<object, object> initialCheck,
                            Func<T, T> finalTransform,
                            ApproachSet<T> approaches)
        {
            this.initialCheck = initialCheck;
            this.finalTransform = finalTransform;
            this.approaches = approaches;
        }

        public T Get<TSource>(Command parent, TSource source)
        {
            if (initialCheck != null)
            {
                source = (TSource)initialCheck(source);
            }
            T ret = approaches.Do(parent, source);
            if (finalTransform != null)
            {
                ret = finalTransform(ret);
            }
            return ret;
        }

        public T Get<TSource, TItem>(Command parent, TSource source, TItem item)
        {
            if (initialCheck != null)
            {
                source = (TSource)initialCheck(source);
            }
            T ret = approaches.Do(parent, source, item);
            if (finalTransform != null)
            {
                ret = finalTransform(ret);
            }
            return ret;
        }
    }

    public abstract class FinderBaseForList<T> : FinderBase<IEnumerable<T>>
    {
        protected FinderBaseForList(Func<object, object> initialCheck = null,
                     Func<IEnumerable<T>, IEnumerable< T>> finalTransform = null,
                     params Approach<IEnumerable<T>>[] approaches)
            : base(initialCheck,finalTransform, ApproachSetForList<T>.Create(approaches))
        {      }
    }
}
