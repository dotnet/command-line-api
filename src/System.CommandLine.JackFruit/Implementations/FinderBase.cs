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
                var typeDef = typeof(ApproachSetForList<>);
                var newType = typeDef.MakeGenericType(new Type[] { itemType });
                var approachSet = Activator.CreateInstance(newType, true);
                return new FinderBase<T>(approaches: approachSet);
            }
            return new FinderBase<T>(approaches: new ApproachSet<T>());
        }
    }

    public class FinderBase<TReturn> :FinderBase, IFinder<TReturn>
    {
        private ApproachSet<TReturn> approaches;
        private Func<object, object> initialCheck;
        private Func<TReturn, TReturn> finalTransform;

   
        internal FinderBase(object approaches)
            => this.approaches = (ApproachSet<TReturn>)approaches;

        internal FinderBase(ApproachSet<TReturn> approaches)
            => this.approaches = approaches;

        public IFinder<TReturn> AddApproach<TSource>(Func<Command[], TSource, (bool, TReturn)> approachFunc)
        {
            approaches.Add(Approach<TReturn>.CreateApproach(approachFunc));
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
            var ret = approaches.Do(parents, source);
            if (finalTransform != null)
            {
                ret = finalTransform(ret);
            }
            return ret;
        }
    }
}
