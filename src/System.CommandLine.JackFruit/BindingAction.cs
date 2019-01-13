using System;
using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.Reflection;
using System.Text;

namespace System.CommandLine.JackFruit
{
    public abstract class BindingActionBase
    {
        private protected BindingActionBase(object reflectionThing, Type returnType)
        {
            ReflectionThing = reflectionThing;
            ReturnType = returnType;
        }

        public object ReflectionThing { get; } // ParameterInfo, PropertyInfo or other
        public Type ReturnType { get; }

    }

    public class SymbolBindingAction : BindingActionBase
    {
        internal SymbolBindingAction(object reflectionThing, Type returnType, ISymbolBase symbol)
            : base(reflectionThing, returnType)
            => Symbol = symbol;

        // In case of redundancy, last one wins. If
        public ISymbolBase Symbol { get; }

        public static SymbolBindingAction Create(ParameterInfo paramInfo, ISymbolBase optionOrArgument)
            => new SymbolBindingAction(paramInfo, paramInfo.ParameterType, optionOrArgument);

        public static SymbolBindingAction Create(PropertyInfo propertyInfo, ISymbolBase optionOrArgument)
            => new SymbolBindingAction(propertyInfo, propertyInfo.PropertyType, optionOrArgument);
    }

    public class FuncBindingAction<TTarget> : BindingActionBase
    {
        internal FuncBindingAction(object reflectionThing, Type returnType,
              Func<InvocationContext, TTarget, object> valueFunc)
                 : base(reflectionThing, returnType)
             => ValueFunc = valueFunc;

        public Func<InvocationContext, TTarget, object> ValueFunc { get; set; }

        public static FuncBindingAction<TTarget> Create<TValue>(PropertyInfo propertyInfo, Func<TValue> valueFunc) 
            => new FuncBindingAction<TTarget>(propertyInfo, typeof(TValue), (c, t) => valueFunc());

        public static FuncBindingAction<TTarget> Create<TValue>(ParameterInfo parameterInfo, Func<TValue> valueFunc) 
            => new FuncBindingAction<TTarget>(parameterInfo, typeof(TValue), (c, t) => valueFunc());

    }
}
