using System.Linq.Expressions;
using System.Reflection;

namespace System.CommandLine.Invocation
{

    public delegate void BindingSetter(InvocationContext context, object target, object value);
    public delegate object BindingGetter(InvocationContext context, object target);

    public abstract class BindingSide
    {
        public BindingSide(BindingGetter get, BindingSetter set)
        {
            Set = set;
            Get = get;
        }
        public BindingSetter Set { get; }
        public BindingGetter Get { get; }
    }

    public class Binding
    {

        public BindingSide TargetSide { get; }
        public BindingSide ParserSide { get; }

        public Binding(BindingSide targetSide, BindingSide parserSide)
        {
            TargetSide = targetSide;
            ParserSide = parserSide;
        }

        public void BindDefaults(InvocationContext context = null, object target = null)
        {
            var value = TargetSide.Get(context, target);
            ParserSide.Set(context, target, value);
        }

        public void Bind(InvocationContext context = null, object target = null)
        {
            var value = ParserSide.Get(context, target);
            TargetSide.Set(context, target, value);
        }


    }

    public class SymbolBindingSide : BindingSide
    {
        private SymbolBindingSide(Option option)
            : base(GetOptionRetrieve(option), GetOptionAssign(option))
            => Symbol = option;
        public SymbolBindingSide(Argument argument)
            : base(GetArgumentRetrieve(argument), GetArgumentAssign(argument))
            => Symbol = argument;

        public static SymbolBindingSide Create(Option symbol)
            => new SymbolBindingSide(symbol);

        public static SymbolBindingSide Create(Argument argument)
            => new SymbolBindingSide(argument);

        public ISymbolBase Symbol { get; }

        private static BindingGetter GetOptionRetrieve(Option option)
            => (context, target) => context.ParseResult.GetValueOrDefault(option);

        private static BindingSetter GetOptionAssign(Option option)
            => (context, target, value) => option.Argument.SetDefaultValue(value);

        private static BindingGetter GetArgumentRetrieve(Argument argument)
            => (context, target) => context.ParseResult.GetValueOrDefault(argument);

        private static BindingSetter GetArgumentAssign(Argument argument)
            => (context, target, value) => argument.SetDefaultValue(value);
    }

    public class ValueBindingSide : BindingSide
    {
        private ValueBindingSide(BindingGetter getter, BindingSetter setter)
               : base(getter, setter)
        { }

        public static ValueBindingSide Create(Expression<Func<object>> valueExpression)
            => throw new NotImplementedException();

        public static ValueBindingSide Create<T>(Func<T> valueGetter, Action<object> valueSetter)
             => new ValueBindingSide((c, t) => valueGetter(), (c, t, value) => valueSetter(value));

        public static ValueBindingSide Create<T>(Func<T> valueGetter)
             => new ValueBindingSide((c, t) => valueGetter(), null);
    }

    public class ServiceBindingSide : BindingSide
    {
        private ServiceBindingSide(BindingGetter getter, BindingSetter setter)
           : base(getter, setter)
        { }

        public static ServiceBindingSide Create(Type serviceType)
             => new ServiceBindingSide((c, t) => c.ServiceProvider.GetService(serviceType),  null);

    }
}
