using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    public class ReflectionBinder : IBinder
    {
        public ReflectionBinder(Type type)
            => Type = type;

        protected const BindingFlags CommonBindingFlags = BindingFlags.FlattenHierarchy
                                    | BindingFlags.IgnoreCase
                                    | BindingFlags.Public
                                    | BindingFlags.NonPublic;

        protected const BindingFlags IgnorePrivateBindingFlags = BindingFlags.FlattenHierarchy
                                | BindingFlags.IgnoreCase
                                | BindingFlags.Public
                                | BindingFlags.NonPublic;

        private object _explicitlySetTarget;
        private Type Type { get; }
        protected internal MethodInfo InvocationMethodInfo { get; private set; }
        // I really hate the location of these. I think we need a reflection binding set that incorporates these
        protected ParameterCollection InvocationParameterCollection { get; private set; }
        protected ParameterCollection ConstructorParameterCollection { get; private set; }

        protected BindingSet ConstructorBindingSet = new BindingSet();
        protected BindingSet InvocationBindingSet = new BindingSet();
        private bool IsBoundToCommand;

        public void AddBinding(object source, Option option)
        {
            switch (source)
            {
                case ParameterInfo parameterInfo:
                    AddBinding(parameterInfo, option);
                    break;
                case PropertyInfo propertyInfo:
                    AddBinding(propertyInfo, option);
                    break;
                default:
                    throw new InvalidOperationException("Internal: Unexpected source type");
            }
        }

        public void AddBinding(object source, Argument argument)
        {
            switch (source)
            {
                case ParameterInfo parameterInfo:
                    AddBinding(parameterInfo, argument);
                    break;
                case PropertyInfo propertyInfo:
                    AddBinding(propertyInfo, argument);
                    break;
                default:
                    throw new InvalidOperationException("Internal: Unexpected source type");
            }
        }

        internal void SetInvocationMethod(MethodInfo methodInfo)
            => InvocationMethodInfo = methodInfo;

        public void AddBinding<T>(object source, Func<T> valueFunc)
        {
            switch (source)
            {
                case ParameterInfo parameterInfo:
                    AddBinding(parameterInfo, valueFunc);
                    break;
                case PropertyInfo propertyInfo:
                    AddBinding(propertyInfo, valueFunc);
                    break;
                default:
                    throw new InvalidOperationException("Internal: Unexpected source type");
            }
        }

        public void AddBinding(ParameterInfo parameterInfo, Option option)
            => AddBinding(parameterInfo, SymbolBindingSide.Create(option));

        public void AddBinding(ParameterInfo parameterInfo, Argument argument)
            => AddBinding(parameterInfo, SymbolBindingSide.Create(argument));

        public void AddBinding<T>(ParameterInfo parameterInfo, Func<T> valueFunc)
            => AddBinding(parameterInfo, ValueBindingSide.Create(valueFunc));

        public void AddBinding(ParameterInfo parameterInfo, BindingSide parserBindingSide)
        {
            var temp = parameterInfo.Member.Name.ToString() == "Install";
            switch (parameterInfo.Member)
            {
                case MethodInfo methodInfo:
                    InvocationMethodInfo = InvocationMethodInfo
                                            ?? methodInfo;
                    InvocationParameterCollection = InvocationParameterCollection
                                            ?? new ParameterCollection(methodInfo);
                    InvocationBindingSet.AddBinding(ParameterBindingSide.Create(
                                parameterInfo, InvocationParameterCollection), parserBindingSide);
                    return;
                case ConstructorInfo constructorInfo:
                    ConstructorParameterCollection = ConstructorParameterCollection
                                            ?? new ParameterCollection(constructorInfo);
                    ConstructorBindingSet.AddBinding(ParameterBindingSide.Create(parameterInfo, ConstructorParameterCollection), parserBindingSide);
                    return;
                default:
                    throw new InvalidOperationException("Internal: Unexpected parameter location");
            }
        }

        public void AddBinding(PropertyInfo propertyInfo, Option option)
           => AddBinding(propertyInfo, SymbolBindingSide.Create(option));

        public void AddBinding(PropertyInfo propertyInfo, Argument argument)
            => AddBinding(propertyInfo, SymbolBindingSide.Create(argument));

        public void AddBinding<T>(PropertyInfo propertyInfo, Func<T> valueFunc)
            => AddBinding(propertyInfo, ValueBindingSide.Create(valueFunc));

        public void AddBinding(PropertyInfo propertyInfo, BindingSide parserBindingSide)
        {
            if (propertyInfo.GetAccessors(true)[0].IsStatic)
            {
                ConstructorBindingSet.AddBinding(PropertyBindingSide.Create(propertyInfo), parserBindingSide);
            }
            else
            {
                InvocationBindingSet.AddBinding(PropertyBindingSide.Create(propertyInfo), parserBindingSide);
            }
        }

        /// <summary>
        /// Create funcioning handler from a method info and it's executing context. Binding can be to services
        /// method defaults and optionally a command that is already populated with symbols.
        /// 
        /// All bindable items are bound - iow, if a symbol could be bound to a property and a parameter, it will be
        /// bound to both
        /// 
        /// After this binding is complete, an attempt will be made to bind unbound parameters to a service by type.
        /// We could do this for properties as well, but are not yet doing that. 
        /// 
        /// Any unbound parameters at this point will attempt to be bound to properties on the target by case  
        /// insensitive name. 
        /// 
        /// If not found, the parameter will be reported as unbound. 
        /// 
        /// Unbound parameters do not result in an error, because there is always a value. If the parameter has a
        /// default value, that is used. If not, the  default value of the type is used
        /// 
        /// This approach was chosen because for explictly bound objects, multiple binding targets are allowed, so
        /// it was desirable to do that here and to say that binding never fails. 
        /// 
        /// When a missing binding should fail, use GetUnboundParameters. There is not yet a GetUnboundConstructorParameters,
        /// partly because that would be problematic if someone has set up DI. But then we don't yet understand the DI 
        /// interaction.
        /// 
        /// </summary>
        /// <param name="methodInfo">The method to be bound</param>
        /// <param name="command">A command containing symbols that can be bound to.</param>
        /// <returns></returns>
        public void AddBindings(MethodInfo methodInfo, ICommand command, bool ignorePrivate = false)
        {
            if (command == null)
            {
                return;
            }
            var type = methodInfo.DeclaringType;
            var bindingFlags = ignorePrivate
                                ? IgnorePrivateBindingFlags
                                : CommonBindingFlags;
            // bind in same order as invocation. not sure this matters
            AddBindingForStaticPropertiesToCommand(type, command, bindingFlags);
            AddBindingForConstructorParametersToCommand(type, command, bindingFlags);
            AddBindingForPropertiesToCommand(type, command, bindingFlags);
            AddBindingForParametersToCommand(methodInfo, command, bindingFlags);
            var unboundParameters = GetUnboundParameters(methodInfo, InvocationBindingSet);
            AddBindingForServiceParameters(unboundParameters);
            unboundParameters = GetUnboundParameters(methodInfo, InvocationBindingSet); // drop any that are now bound
            AddBindingForPropertyParameters(type, unboundParameters);
            IsBoundToCommand = true;
        }

        internal void AddBindingsIfNeeded(ICommand command)
        {
            // I am not crazy about using a boolean. It won't play nice with mixed 
            // mode binding (manual and automatic)
            if (!IsBoundToCommand)
            {
                AddBindings(InvocationMethodInfo, command);
            }
        }

        public void SetTarget(object target)
            => _explicitlySetTarget = target;

        public object GetTarget(InvocationContext context = null)
        {
            ConstructorBindingSet.Bind(context, null);
            var target = _explicitlySetTarget != null
                         ? _explicitlySetTarget
                         : Activator.CreateInstance(Type,
                            JustGetConstructorArguments());
            InvocationBindingSet.Bind(context, target);
            return target;
        }

        private object[] JustGetConstructorArguments()
            => HandleNullArguments(ConstructorParameterCollection?.GetArguments());

        private object[] JustGetInvocationArguments()
            => HandleNullArguments(InvocationParameterCollection?.GetArguments());

        public object[] GetConstructorArguments(InvocationContext context = null)
        {
            ConstructorBindingSet.Bind(context, null);
            return JustGetConstructorArguments();
        }

        private object[] HandleNullArguments(object[] arguments)
            => arguments
               ?? Array.Empty<object>();

        public object[] GetInvocationArguments(InvocationContext context)
        {
            // It may not be necessary to get the target first
            var target = GetTarget(context);
            return JustGetInvocationArguments();
        }

        public object InvokeAsync(InvocationContext context)
        {
            var target = GetTarget(context);
            // Invocation bind is done during Target construction (to allow dependency on properties)
            var arguments = JustGetInvocationArguments();
            var value = InvocationMethodInfo.Invoke(target, arguments);
            return CommandHandler.GetResultCodeAsync(value, context);
        }

        // If a corresponding symbol isn't fuond, that's fine Invocation should use default
        // Add a way to include an explicit default from parameter info when there is no option
        // Add a "strict" method test that warns on missing symbols. Levels including ignore static properties
        private void AddBindingForPropertyParameters(Type type, IEnumerable<ParameterInfo> unboundParameters)
        {
            foreach (var parameterInfo in unboundParameters)
            {
                var matchingProperties = type.GetProperties(CommonBindingFlags | BindingFlags.Static | BindingFlags.Instance)
                                        .Where(p => p.Name.Equals(parameterInfo.Name, StringComparison.InvariantCultureIgnoreCase));
                matchingProperties = matchingProperties.Count() <= 1
                                     ? matchingProperties
                                     : matchingProperties
                                                .Where(p => p.Name == parameterInfo.Name);
                if (matchingProperties.Any())
                {
                    AddBinding(parameterInfo, PropertyBindingSide.Create(matchingProperties.First()));
                }
            }
        }

        private void AddBindingForServiceParameters(IEnumerable<ParameterInfo> unboundParameters)
        {
            var bindable = unboundParameters
                            .Where(p => InvocationContext.AvailableServiceTypes
                                        .Contains(p.ParameterType));
            foreach (var parameterInfo in bindable)
            {
                AddBinding(parameterInfo, ServiceBindingSide.Create(parameterInfo.ParameterType));
            }
        }

        public IEnumerable<ParameterInfo> GetUnboundParameters(MethodInfo methodInfo,
                    BindingSet bindingSet, bool considerParameterDefaults = false)
            => methodInfo.GetParameters()
                    .Where(p => (considerParameterDefaults && p.HasDefaultValue) // always considered bound
                                || bindingSet.FindTargetBinding<ParameterBindingSide>(pbs => pbs.ParameterInfo == p)
                                    .None());

        private void AddBindingForParametersToCommand(MethodInfo methodInfo, ICommand command, BindingFlags bindingFlags)
        {
            AddBindingForMethodBase(methodInfo, command, bindingFlags);
        }

        private void AddBindingForPropertiesToCommand(Type type, ICommand command, BindingFlags bindingFlags)
        {
            var properties = type.GetProperties(bindingFlags)
                                  .Where(p => !(p.GetAccessors().FirstOrDefault()?.IsStatic).GetValueOrDefault());
            foreach (var property in properties)
            {
                AddBinding(property, command);
            }
        }

        private void AddBindingForConstructorParametersToCommand(Type type, ICommand command, BindingFlags bindingFlags)
        {
            var ctors = type.GetConstructors(bindingFlags);
            switch (ctors.Count())
            {
                case 0: // do not need constructor
                    return;
                case 1:
                    AddBindingForMethodBase(ctors.First(), command, bindingFlags);
                    break;
                default:
                    // TODO: This is probably wrong. We should have picking rules I think 
                    throw new InvalidOperationException("Internal: Currently bound types can have only one constructor");
            }

        }

        private void AddBindingForMethodBase(MethodBase methodBase, ICommand command, BindingFlags bindingFlags)
        {
            var parameters = methodBase.GetParameters();
            foreach (var parameterInfo in parameters)
            {
                AddBinding(parameterInfo, command);
            }
        }

        private void AddBindingForStaticPropertiesToCommand(Type type, ICommand command, BindingFlags bindingFlags)
        {
            var properties = type.GetProperties(bindingFlags)
                              .Where(p => (p.GetAccessors().FirstOrDefault()?.IsStatic).GetValueOrDefault());
            foreach (var property in properties)
            {
                AddBinding(property, command);
            }
        }

        // TODO: Candidates for a base class
        private void AddBinding(PropertyInfo propertyInfo, ICommand command)
        {
            var symbol = FindMatchingSymbol(propertyInfo.Name, command);
            switch (symbol)
            {
                case null:
                    return;
                case Argument argument:
                    AddBinding(propertyInfo, argument);
                    break;
                case Option option:
                    AddBinding(propertyInfo, option);
                    break;
                default:
                    throw new InvalidOperationException("Internal: Unexpected symbol type");
            }
        }

        private void AddBinding(ParameterInfo parameterInfo, ICommand command)
        {
            var symbol = FindMatchingSymbol(parameterInfo.Name, command);
            switch (symbol)
            {
                case null:
                    return;
                case Argument argument:
                    AddBinding(parameterInfo, argument);
                    break;
                case Option option:
                    AddBinding(parameterInfo, option);
                    break;
                default:
                    throw new InvalidOperationException("Internal: Unexpected symbol type");
            }
        }

        private static ISymbolBase FindMatchingSymbol(string name, ICommand command)
           => command?.Children.GetByAlias(name.ToKebabCase().ToLowerInvariant());

        internal static bool IsMatch(string parameterName, string alias) =>
            string.Equals(alias?.RemovePrefix()
                               .FromKebabCase(),
                          parameterName,
                          StringComparison.OrdinalIgnoreCase);

        internal static bool IsMatch(string parameterName, ISymbol symbol) =>
            symbol.Aliases.Any(parameterName.IsMatch);

        void IBinder.AddBinding(Binding binding)
            => InvocationBindingSet.AddBinding(binding);

        void IBinder.AddBinding(BindingSide targetSide, BindingSide parserSide)
            => InvocationBindingSet.AddBinding(targetSide, parserSide);
    }
}
