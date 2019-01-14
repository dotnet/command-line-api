using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation
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
        public BindingSet BindingSet { get; } = new BindingSet();

        public static ReflectionCommandHandler Create(MethodInfo methodInfo)
        {
            var handler = CreateInternal(methodInfo.DeclaringType, methodInfo, null);
            return handler;
        }

        public static ReflectionCommandHandler Create(Type declaringType)
        {
            var handler = CreateInternal(declaringType, null, null);
            return handler;
        }

        public static ReflectionCommandHandler Create(MethodInfo methodInfo, object target)
        {
            var handler = CreateInternal(methodInfo.DeclaringType, methodInfo, target);
            return handler;
        }

        private static ReflectionCommandHandler CreateInternal(Type declaringType, MethodInfo methodInfo, object target)
        {
            var genericMethod = typeof(ReflectionCommandHandler).GetMethods()
                                    .Where(x => x.Name == "Create" && x.IsGenericMethod
                                            && x.GetParameters().FirstOrDefault()?.ParameterType == typeof(MethodInfo))
                                    .First();
            var constructedMethod = genericMethod.MakeGenericMethod(declaringType);
            var handler = constructedMethod.Invoke(null, new object[] { methodInfo, target });
            return (ReflectionCommandHandler)handler;
        }

        public static ReflectionCommandHandler<TTarget> Create<TTarget>(MethodInfo methodInfo = null, TTarget target = null)
                where TTarget : class
        {
            var handler = Activator.CreateInstance<ReflectionCommandHandler<TTarget>>();
            handler.Target = target;
            methodInfo = methodInfo ?? GetInvokeMethod(typeof(TTarget));
            handler.InvocationMethodInfo = methodInfo;
            return handler;
        }

        private static MethodInfo GetInvokeMethod(Type type)
        {
            var methodInfo = type.GetMethod("InvokeAsync");
            return methodInfo ?? type.GetMethods()
                                     .Where(x => x.Name.StartsWith("Invoke"))
                                     .FirstOrDefault();
        }

        public void AddBinding(BindingBase bindingAction)
            => BindingSet.AddBinding(bindingAction);

        public void AddBindings(IEnumerable<BindingBase> bindingActions)
            => BindingSet.AddBindings(bindingActions);

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
        public TTarget Target { get; set; }
        private InvocationContext context;
        public Func<InvocationContext, TTarget> CreateTargetFunc { get; set; }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        // TODO: How do we await this via reflection?
        public override Task<int> InvokeAsync(InvocationContext context)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            this.context = context;
            SetStaticProperties();

            var target = Target
                         ?? (CreateTargetFunc == null
                            ? CreateTarget()
                            : CreateTargetFunc?.Invoke(context));

            SetTargetProperties(target);

            var methodArguments = GetParameterValues(target, InvocationMethodInfo);

            var value = InvocationMethodInfo.Invoke(target, methodArguments);

            // release reference to context, since it's only valid during invoke
            this.context = null;

            return CommandHandler.GetResultCodeAsync(value, context);

            //switch (ret)
            //{
            //    case int i:
            //        return i;
            //    case Task<int> task:
            //        return await task;
            //    default:
            //        return default(int);
            //}
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
                var binding = BindingSet.Find(param);
                values.Add(GetParameterValue(target, context, param, binding));
            }
            return values.ToArray();
        }

        private object GetParameterValue(TTarget target, InvocationContext context, ParameterInfo param, BindingBase binding)
        {
            if (binding != null)
            {
                return ValueFromBinding(target, binding);
            }
            var value = context.ServiceProvider.GetService(param.ParameterType);
            return value ?? (param.HasDefaultValue
                                ? param.DefaultValue
                                : param.ParameterType.GetDefaultValue());
        }

        private object ValueFromBinding(TTarget target, BindingBase binding)
        {
            object value;
            switch (binding)
            {
                case FuncBinding<TTarget> funcBinding:
                    value = funcBinding.ValueFunc(context, target);
                    break;
                case SymbolBinding symbolBinding:
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
                var binding = BindingSet.Find(prop);
                object value = binding == null
                                ? context.ServiceProvider.GetService(prop.PropertyType)
                                : ValueFromBinding(null, binding);
                prop.SetValue(target, value);
            }
        }
    }
}
