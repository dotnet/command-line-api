using System;
using System.Collections.Generic;
using System.Text;

namespace System.CommandLine.JackFruit
{
    // TODO: CLenaup or collapse FinderForList into Finder
    // TODO: Consider making initialCheck and finalTransform not optional and requiring derived to pass null
    public abstract class FinderBase<T> : IFinder<T>
    {
        private ApproachSet<T> approaches;
        private Func<object, object> initialCheck;
        private Func<T, T> finalTransform;

        protected FinderBase(Func<object, object> initialCheck = null,
                            Func<T, T> finalTransform = null,
                            params Approach<T>[] approaches)
        {
            this.initialCheck = initialCheck;
            this.finalTransform = finalTransform;
            this.approaches = new ApproachSet<T>(approaches);
        }

        public void AddApproach(Approach<T> approach)
            => approaches.Add(approach);

        public T Get<TSource>(TSource source)
        {
            if (initialCheck != null)
            {
                source = (TSource)initialCheck(source);
            }
            T ret = approaches.Do(source);
            if (finalTransform != null)
            {
                ret = finalTransform(ret);
            }
            return ret;
        }

        public T Get<TSource, TItem>(TSource source, TItem item)
        {
            if (initialCheck != null)
            {
                source = (TSource)initialCheck(source);
            }
            T ret = approaches.Do(source, item);
            if (finalTransform != null)
            {
                ret = finalTransform(ret);
            }
            return ret;
        }
    }

    public abstract class FinderForListBase<T> : IListFinder<T>
    {
        private ApproachSetForList<T> approaches;
        private Func<object, object> initialCheck;
        private Func<IEnumerable<T>, IEnumerable<T>> finalTransform;

        public void AddApproach(Approach<IEnumerable<T>> approach)
            => approaches.Add(approach);

        protected FinderForListBase(Func<object, object> initialCheck = null,
                     Func<IEnumerable<T>, IEnumerable<T>> finalTransform = null,
                     params Approach<IEnumerable<T>>[] approaches)
        {
            this.initialCheck = initialCheck;
            this.finalTransform = finalTransform;
            this.approaches = new ApproachSetForList<T>(approaches);
        }

        public IEnumerable<T> Get<TSource>(TSource source)
        {
            if (initialCheck != null)
            {
                source = (TSource)initialCheck(source);
            }
            IEnumerable<T> ret = approaches.Do(source);
            if (finalTransform != null)
            {
                ret = finalTransform(ret);
            }
            return ret;
        }
    }
}
