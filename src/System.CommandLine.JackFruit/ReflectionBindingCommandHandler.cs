using System;
using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System.CommandLine.JackFruit
{
    // You can get data from at least the following places
    // - unrelated to CommandLine, like an environment variable
    // - somewhere else in the result tree
    // - options and arguments (symbols) on this command

    // You always invoke in a context that is
    // - an operating system environment, including environment variables
    // - a running application and any statics on other classes, icnluding service provider/DI
    // - a type, it's properties, static properties and partent prporties/static properties
    //   - the type was created with a constructor that may have had parameters
    // - a method and it's parameters

    // In the case where we have a method info, everything else is available
    // I am designing for the class containing the method to be special purpose 
    //      and call into the rest of the app, for exammple, demanding one constructor

    public abstract class ReflectionCommandHandler : ICommandHandler
    {
        public List<BindingActionBase> BindActions { get; } = new List<BindingActionBase>();

        public static ReflectionCommandHandler Create(MethodInfo methodInfo)
        {
            var declaringType = methodInfo.DeclaringType;
            var genericReflectionType = typeof(ReflectionCommandHandler<>);
            var constructedType = genericReflectionType.MakeGenericType(genericReflectionType);
            var handler = (ReflectionCommandHandler)Activator.CreateInstance(constructedType);
            handler.InvocationMethodInfo = methodInfo;
            return handler;
        }

        public static ReflectionCommandHandler<TTarget> Create<TTarget>(MethodInfo methodInfo)
                where TTarget : class
        {
            var handler = Activator.CreateInstance<ReflectionCommandHandler<TTarget>>();
            handler.InvocationMethodInfo = methodInfo;
            return handler;
        }

        public static ReflectionCommandHandler<TTarget> Create<TTarget>()
                where TTarget : class
        {
            var methodInfo = typeof(TTarget).GetMethod("InvokeAsync");
            var handler = Activator.CreateInstance<ReflectionCommandHandler<TTarget>>();
            handler.InvocationMethodInfo = methodInfo;
            return handler;
        }

        public void AddBinding(BindingActionBase bindingAction) 
            => BindActions.Add(bindingAction);

        public void AddBindings(IEnumerable<BindingActionBase> bindingActions)
            => BindActions.AddRange(bindingActions);

        public abstract Task<int> InvokeAsync(InvocationContext context);

        protected MethodInfo InvocationMethodInfo { get; set; }
    }

    public class ReflectionCommandHandler<TTarget> : ReflectionCommandHandler
        where TTarget : class
    {
        private const BindingFlags CommonBindingFlags = BindingFlags.FlattenHierarchy
                                            | BindingFlags.IgnoreCase
                                            | BindingFlags.Public
                                            | BindingFlags.NonPublic;
        private InvocationContext context;
        public Func<InvocationContext, TTarget> CreateTargetFunc { get; set; }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        // TODO: How do we await this via reflection?
        public override async Task<int> InvokeAsync(InvocationContext context)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            this.context = context;
            SetStaticProperties();

            var target = CreateTargetFunc == null
                            ? CreateTarget()
                            : CreateTargetFunc?.Invoke(context);

            SetTargetProperties(target);

            var methodArguments = GetParameterValues(target, InvocationMethodInfo);

            var ret = InvocationMethodInfo.Invoke(target, methodArguments);
            this.context = null;
            if (ret is Task<int> task)
            {
                return await task;
            }
            return (int)ret;
        }

        private void SetTargetProperties(TTarget target)
        {
            var type = typeof(TTarget);
            var properties = type.GetProperties(CommonBindingFlags | BindingFlags.Instance);
            SetProperties(target, properties);
        }

        private TTarget CreateTarget()
        {
            var type = typeof(TTarget);
            // TODO: Figure out if we should restrict to public constructors
            var ctors = type.GetConstructors();
            if (ctors.Count() > 1)
            {
                throw new InvalidOperationException("Target cannot have multiple constructors");
            }
            var ctor = ctors.FirstOrDefault();
            return ctor.GetParameters().Count() == 0
                    ? Activator.CreateInstance<TTarget>()
                    : GetTargetFromConstructor(ctor);
        }

        private TTarget GetTargetFromConstructor(ConstructorInfo constructorInfo)
        {
            var type = typeof(TTarget);
            var paramValues = GetParameterValues(null, constructorInfo);
            return (TTarget)Activator.CreateInstance(type, paramValues);
        }

        private object[] GetParameterValues(TTarget target, MethodBase methodInfo)
        {
            var parameters = methodInfo.GetParameters();
            var values = new List<object>();
            foreach (var param in parameters)
            {
                var binding = BindActions
                                .Where(x => x.ReflectionThing.Equals(param))
                                .LastOrDefault();
                var value = binding == null
                            ? param.HasDefaultValue
                                ? param.DefaultValue
                                : param.ParameterType.GetDefaultValue()
                            : ValueFromBinding(target, binding);
                values.Add(value);
            }
            return values.ToArray();
        }

        private object ValueFromBinding(TTarget target, BindingActionBase binding)
        {
            object value;
            switch (binding)
            {
                case FuncBindingAction<TTarget> funcBinding:
                    value = funcBinding.ValueFunc(context, target);
                    break;
                case SymbolBindingAction symbolBinding:
                    switch (symbolBinding.Symbol)
                    {
                        case Option option:
                            value = context.ParseResult.GetValue(option);
                            break;
                        case Argument argument:
                            value = context.ParseResult.GetValue(argument);
                            break;
                        default:
                            throw new InvalidOperationException("Internal: Unknown symbol type encountered");
                    }
                    break;
                default:
                    throw new InvalidOperationException("Internal: Symbol or value func missing in BindAction");
            }
            // TODO: Handle conversion errors - needs to propagate so needs thought
            var typedValue = Convert.ChangeType(value, binding.ReturnType);
            return typedValue;
        }

        private void SetStaticProperties()
        {
            var bindingFlags = CommonBindingFlags | BindingFlags.Static;
            var type = typeof(TTarget);
            var staticProperties = type.GetProperties(bindingFlags);
            SetProperties(null, staticProperties);
            if (type.IsNested)
            {
                // TODO: How do you find a nested types parent?
                // TODO: Set nested parents static properties
            }
        }

        private void SetProperties(object target, IEnumerable<PropertyInfo> properties)
        {
            foreach (var prop in properties)
            {
                var binding = BindActions
                                .Where(x => x.ReflectionThing.Equals(prop))
                                .LastOrDefault();

                if (binding != null)
                {
                    var value = ValueFromBinding(null, binding);
                    prop.SetValue(target, value);
                }
            }
        }
    }

}
