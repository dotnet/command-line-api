using System;
using System.Collections.Generic;
using System.CommandLine.Invocation;
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

    public class SymbolBindingAction :BindingActionBase
    {
        internal SymbolBindingAction(object reflectionThing, Type returnType, ISymbolBase symbol)
            : base(reflectionThing, returnType)
            => Symbol = symbol;

        // In case of redundancy, last one wins. If
        public ISymbolBase Symbol { get; }
    }

    public class FuncBindingAction<TTarget> : BindingActionBase
    {
        internal FuncBindingAction(object reflectionThing, Type returnType,
              Func<InvocationContext, TTarget, object> valueFunc)
         : base(reflectionThing, returnType)
         => ValueFunc = valueFunc;

        public Func<InvocationContext, TTarget, object> ValueFunc { get; set; }
    }
}
