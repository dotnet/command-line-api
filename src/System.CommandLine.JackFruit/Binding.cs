using System;
using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.Reflection;
using System.Text;

namespace System.CommandLine.JackFruit
{
    public abstract class BindingBase
    {
        private protected BindingBase(object reflectionThing, Type returnType)
        {
            ReflectionThing = reflectionThing;
            ReturnType = returnType;
        }

        public object ReflectionThing { get; } // ParameterInfo, PropertyInfo or other
        public Type ReturnType { get; }

    }

    public class SymbolBinding : BindingBase
    {
        internal SymbolBinding(object reflectionThing, Type returnType, ISymbolBase symbol)
            : base(reflectionThing, returnType)
            => Symbol = symbol;

        // In case of redundancy, last one wins. If
        public ISymbolBase Symbol { get; }

        public static SymbolBinding Create(ParameterInfo paramInfo, ISymbolBase optionOrArgument)
            => new SymbolBinding(paramInfo, paramInfo.ParameterType, optionOrArgument);

        public static SymbolBinding Create(PropertyInfo propertyInfo, ISymbolBase optionOrArgument)
            => new SymbolBinding(propertyInfo, propertyInfo.PropertyType, optionOrArgument);
    }

    public class FuncBinding<TTarget> : BindingBase
    {
        internal FuncBinding(object reflectionThing, Type returnType,
              Func<InvocationContext, TTarget, object> valueFunc)
                 : base(reflectionThing, returnType)
             => ValueFunc = valueFunc;

        public Func<InvocationContext, TTarget, object> ValueFunc { get; set; }

        public static FuncBinding<TTarget> Create<TValue>(PropertyInfo propertyInfo, Func<TValue> valueFunc) 
            => new FuncBinding<TTarget>(propertyInfo, typeof(TValue), (c, t) => valueFunc());

        public static FuncBinding<TTarget> Create<TValue>(ParameterInfo parameterInfo, Func<TValue> valueFunc) 
            => new FuncBinding<TTarget>(parameterInfo, typeof(TValue), (c, t) => valueFunc());

    }
}
