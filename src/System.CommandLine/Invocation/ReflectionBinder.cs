using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace System.CommandLine.Invocation
{
    public class ReflectionBinder
    {
        protected const BindingFlags CommonBindingFlags = BindingFlags.FlattenHierarchy
                                    | BindingFlags.IgnoreCase
                                    | BindingFlags.Public
                                    | BindingFlags.NonPublic;

    }

    public class ReflectionBinder<TTarget> : ReflectionBinder
        where TTarget : class
    {
        public BindingSet BindingSet { get; } = new BindingSet();
        public Func<InvocationContext, TTarget> CreateTargetFunc { get; set; }
        public TTarget Target { get; set; }

        public void AddBinding(BindingBase bindingAction)
            => BindingSet.AddBinding(bindingAction);

        public void AddBindings(IEnumerable<BindingBase> bindingActions)
            => BindingSet.AddBindings(bindingActions);

        public BindingBase Find(object reflectionObject)
            => BindingSet.Find(reflectionObject);

        public TTarget CreateInstance(InvocationContext context)
        {
            SetStaticProperties(context);
            var target = Target
                         ?? (CreateTargetFunc == null
                            ? CreateTarget(context)
                            : CreateTargetFunc?.Invoke(context));

            SetTargetProperties(context,target);
            return target;
        }

        private void SetTargetProperties(InvocationContext context, TTarget target)
        {
            var type = typeof(TTarget);
            var properties = type.GetProperties(CommonBindingFlags | BindingFlags.Instance);
            SetProperties(context, target, properties);
        }

        private TTarget CreateTarget(InvocationContext context)
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
                    : GetTargetFromConstructor(context, ctor);
        }

        private TTarget GetTargetFromConstructor(InvocationContext context, ConstructorInfo constructorInfo)
        {
            var type = typeof(TTarget);
            var paramValues = GetParameterValues(context, null, constructorInfo);
            return (TTarget)Activator.CreateInstance(type, paramValues);
        }

        public object[] GetParameterValues(InvocationContext context, TTarget target, MethodBase methodInfo)
        {
            var parameters = methodInfo.GetParameters();
            var values = new List<object>();
            foreach (var param in parameters)
            {
                var binding = BindingSet.Find(param);
                values.Add(GetParameterValue(context, target, param, binding));
            }
            return values.ToArray();
        }

        private object GetParameterValue(InvocationContext context,TTarget target,  ParameterInfo param, BindingBase binding)
        {
            if (binding != null)
            {
                return ValueFromBinding(context, target, binding);
            }
            var value = context.ServiceProvider.GetService(param.ParameterType);
            return value ?? (param.HasDefaultValue
                                ? param.DefaultValue
                                : param.ParameterType.GetDefaultValue());
        }

        private object ValueFromBinding(InvocationContext context, TTarget target, BindingBase binding)
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

        private void SetStaticProperties(InvocationContext context)
        {
            var bindingFlags = CommonBindingFlags | BindingFlags.Static;
            var type = typeof(TTarget);
            var staticProperties = type.GetProperties(bindingFlags);
            SetProperties(context, null, staticProperties);
            if (type.IsNested)
            {
                // TODO: How do you find a nested types parent?
                // TODO: Set nested parents static properties
            }
        }

        private void SetProperties(InvocationContext context, object target, IEnumerable<PropertyInfo> properties)
        {
            foreach (var prop in properties)
            {
                var binding = BindingSet.Find(prop);
                object value = binding == null
                                ? context.ServiceProvider.GetService(prop.PropertyType)
                                : ValueFromBinding(context, null, binding);
                prop.SetValue(target, value);
            }
        }

    }
}
