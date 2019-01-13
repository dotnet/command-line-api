//using System;
//using System.Collections.Generic;
//using System.CommandLine.Invocation;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;

//namespace System.CommandLine.JackFruit
//{
//    public abstract class BindingCommandHandler<TThis, TTarget, TStore> : ICommandHandler
//        where TThis : BindingCommandHandler<TThis, TTarget, TStore>
//        where TTarget : class
//    {
//        private List<Action<InvocationContext, TTarget, TStore>> BindActions { get; }

//        public BindingCommandHandler()
//        {
//            BindActions = new List<Action<InvocationContext, TTarget, TStore>>();
//        }

//        public Func<TTarget> CreateTargetFunc { get; set; }
//        protected virtual TStore CreateStoreFunc()
//            => Activator.CreateInstance<TStore>();

//        protected TThis AddBindingAction(Action<InvocationContext, TTarget, TStore> action)
//        {
//            BindActions.Add(action);
//            return (TThis)this;
//        }

//        public Func<InvocationContext, TTarget, TStore, Task<int>> InvokeFunc { get; set; }

//        public async Task<int> InvokeAsync(InvocationContext context)
//        {
//            var target = CreateTargetFunc?.Invoke();
//            var store = CreateStoreFunc();
//            foreach (var action in BindActions)
//            {
//                action(context, target, store);
//            }
//            return await InvokeFunc(context, target, store);
//        }
//    }

//    public abstract class ListStoreBindingCommandHandler<TThis, TTarget> 
//            : BindingCommandHandler<TThis, TTarget, List<object>>
//                 where TThis : ListStoreBindingCommandHandler<TThis, TTarget>
//                 where TTarget : class
//    {
//        private List<object> listObj;

//        public ListStoreBindingCommandHandler()
//        {
//            listObj = new List<object>();
//        }

//        protected TThis AddListStoreBindingAction(Func<InvocationContext, TTarget, object> valueFunc)
//        {
//            var pos = listObj.Count();
//            return AddBindingAction((context, target, store) => store[pos] = valueFunc);

//        }

//        protected override List<object> CreateStoreFunc() 
//            =>  listObj;
//    }

//    public class TypeCreationCommandHandler<TTarget> : BindingCommandHandler<TypeCreationCommandHandler<TTarget>, TTarget, List<object>>
//             where TTarget : class
//    {
//        private TTarget instance;
//        public TypeCreationCommandHandler()
//        {
//            CreateTargetFunc = null;
//            InvokeFunc = (_, _2, store) =>
//                {
//                    instance = (TTarget)Activator.CreateInstance(typeof(TTarget), true, store);
//                    return Task.FromResult(0);
//                };
//        }

//        public TTarget Create()
//        {
//            var _ = InvokeAsync(null).Result;
//            return instance;
//        }
//    }

//    public class MethodBindingCommandHandler<TTarget> 
//        : ListStoreBindingCommandHandler<MethodBindingCommandHandler<TTarget>, TTarget>
//        where TTarget : class
//    {
//        public MethodBindingCommandHandler() => CreateTargetFunc = ()
//            => TypeCreatorHandler.Create();

//        public MethodBindingCommandHandler<TTarget> AddBinding<T>(ParameterInfo paramInfo, Func<T> valueFunc)
//        {
//            AddBindingAction((context, target, store) => store.Add(valueFunc()));
//            return this;
//        }

//        public MethodBindingCommandHandler<TTarget> AddBinding<T>(PropertyInfo propertyInfo, Func<T> valueFunc)
//        {
//            AddBindingAction((context, target, store) => propertyInfo.SetValue(target, valueFunc()));
//            return this;
//        }

//        public MethodBindingCommandHandler<TTarget> AddStaticBinding<T>(PropertyInfo propertyInfo, Func<T> valueFunc)
//        {
//            AddBindingAction((context, _, store) => propertyInfo.SetValue(null, valueFunc()));
//            return this;
//        }

//        public MethodBindingCommandHandler<TTarget> AddBinding<T>(ParameterInfo paramInfo, Option option)
//        {
//            AddBindingAction((context, target, store) => store.Add(context.ParseResult.GetValue(option)));
//            return this;
//        }

//        public MethodBindingCommandHandler<TTarget> AddPropertyBinding<T>(PropertyInfo propertyInfo, Option option)
//        {
//            AddBindingAction((context, target, store) => propertyInfo.SetValue(target, context.ParseResult.GetValue(option)));
//            return this;
//        }

//        public MethodBindingCommandHandler<TTarget> AddStaticBinding<T>(PropertyInfo propertyInfo, Option option)
//        {
//            AddBindingAction((context, _, store) => propertyInfo.SetValue(null, context.ParseResult.GetValue(option)));
//            return this;
//        }

//        public MethodBindingCommandHandler<TTarget> AddBinding<T>(ParameterInfo paramInfo, Argument argument)
//        {
//            var pos = CreateStoreFunc.Length
//            AddBindingAction((context, target, store) => store[2] = context.ParseResult.GetValue(argument));
//            return this;
//        }

//        public MethodBindingCommandHandler<TTarget> AddBinding<T>(PropertyInfo propertyInfo, Argument argument)
//        {
//            AddBindingAction((context, target, store) => propertyInfo.SetValue(target, context.ParseResult.GetValue(argument)));
//            return this;
//        }

//        public MethodBindingCommandHandler<TTarget> AddStaticBinding<T>(PropertyInfo propertyInfo, Argument argument)
//        {
//            AddBindingAction((context, _, store) => propertyInfo.SetValue(null, context.ParseResult.GetValue(argument)));
//            return this;
//        }

//        public TypeCreationCommandHandler<TTarget> TypeCreatorHandler { get; } = new TypeCreationCommandHandler<TTarget>();

//    }
//}
