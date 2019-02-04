// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.Linq;
using System.Reflection;

namespace System.CommandLine.Binding
{
    public class ReflectionBinder : IBinder
    {
        public ReflectionBinder(Type type)
            => Type = type;

        private const BindingFlags CommonBindingFlags = BindingFlags.FlattenHierarchy
                                    | BindingFlags.IgnoreCase
                                    | BindingFlags.Public
                                    | BindingFlags.NonPublic;

        private const BindingFlags IgnorePrivateBindingFlags = BindingFlags.FlattenHierarchy
                                | BindingFlags.IgnoreCase
                                | BindingFlags.Public
                                | BindingFlags.NonPublic;

        private object _explicitlySetTarget;
        private Type Type { get; }
        private  MethodInfo InvocationMethodInfo { get;  set; }
        // I really hate the location of these. I think we need a reflection binding set that incorporates these
        private ParameterCollection InvocationParameterCollection { get;  set; }
        private ParameterCollection ConstructorParameterCollection { get;  set; }

        private readonly BindingSet ConstructorBindingSet = new BindingSet();
        private readonly BindingSet InvocationBindingSet = new BindingSet();
        private bool isBoundToCommand;

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
                    throw new InvalidOperationException("Unexpected source type");
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
                    throw new InvalidOperationException("Unexpected source type");
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
                    throw new InvalidOperationException("Unexpected source type");
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
                    throw new InvalidOperationException("Unexpected parameter location");
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
            var propertyBindingSide = new PropertyBindingSide(propertyInfo);
            if (propertyInfo.GetAccessors(true)[0].IsStatic)
            {
                ConstructorBindingSet.AddBinding(propertyBindingSide, parserBindingSide);
            }
            else
            {
                InvocationBindingSet.AddBinding(propertyBindingSide, parserBindingSide);
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
        public void AddBindings(Type type, MethodInfo methodInfo, ICommand command, bool ignorePrivate = false)
        {
            if (command == null)
            {
                return;
            }
            type = type ?? methodInfo.DeclaringType;
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
            isBoundToCommand = true;
        }

        internal void AddBindingsIfNeeded(ICommand command)
        {
            // I am not crazy about using a boolean. It won't play nice with mixed 
            // mode binding (manual and automatic)
            if (!isBoundToCommand)
            {
                AddBindings(Type, InvocationMethodInfo, command);
            }
        }

        public void SetTarget(object target)
            => _explicitlySetTarget = target;

        private object[] JustGetConstructorArguments()
          => HandleNullArguments(ConstructorParameterCollection?.GetArguments());

        private object[] JustGetInvocationArguments()
            => HandleNullArguments(InvocationParameterCollection?.GetArguments());

        public object[] GetConstructorArguments(InvocationContext context = null)
        {
            AddBindingsIfNeeded(context?.ParseResult?.CommandResult?.Command);
            ConstructorBindingSet.Bind(context, null);
            return JustGetConstructorArguments();
        }

        private static object[] HandleNullArguments(object[] arguments)
            => arguments
               ?? Array.Empty<object>();

        public object GetTarget(InvocationContext context = null)
        {
            AddBindingsIfNeeded(context?.ParseResult?.CommandResult?.Command);
            ConstructorBindingSet.Bind(context, null);
            var target = _explicitlySetTarget != null
                         ? _explicitlySetTarget
                         : Activator.CreateInstance(Type,
                            JustGetConstructorArguments());
            InvocationBindingSet.Bind(context, target);
            return target;
        }

        public object[] GetInvocationArguments(InvocationContext context)
        {
            AddBindingsIfNeeded(context?.ParseResult?.CommandResult?.Command);
            // TODO: There is currently a side effect of GetTarget that needs to be remmoved, but this is currently needed
            var target = GetTarget(context);
            return JustGetInvocationArguments();
        }

        public object InvokeAsync(InvocationContext context)
        {
            AddBindingsIfNeeded(context?.ParseResult?.CommandResult?.Command);
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
                const BindingFlags bindingFlags = CommonBindingFlags | BindingFlags.Static | BindingFlags.Instance;
                IEnumerable<PropertyInfo> matchingProperties = type.GetProperties(bindingFlags)
                                        .Where(p => p.Name.Equals(parameterInfo.Name, StringComparison.InvariantCultureIgnoreCase))
                                        .ToList();
                matchingProperties = matchingProperties.Count() <= 1
                                     ? matchingProperties
                                     : matchingProperties
                                                .Where(p => p.Name == parameterInfo.Name);
                if (matchingProperties.Any())
                {
                    AddBinding(parameterInfo, new PropertyBindingSide(matchingProperties.First()));
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
        {
            if (methodInfo == null)
            {
                return new List<ParameterInfo>();
            }
            return methodInfo.GetParameters()
                               .Where(p => (considerParameterDefaults && p.HasDefaultValue) // always considered bound
                                           || bindingSet.FindTargetBinding<ParameterBindingSide>(pbs => pbs.ParameterInfo == p)
                                               .None());
        }

        private void AddBindingForParametersToCommand(MethodInfo methodInfo, ICommand command, BindingFlags bindingFlags)
        {
            AddBindingForMethodBase(methodInfo, command, bindingFlags);
        }

        private void AddBindingForPropertiesToCommand(Type type, ICommand command, BindingFlags bindingFlags)
        {
            PropertyInfo[] propertyInfos = type.GetProperties(bindingFlags | BindingFlags.Instance);
            var properties = propertyInfos
                            .Where(p => !IsStatic(p) && p.CanWrite);
            foreach (var property in properties)
            {
                AddBinding(property, command);
            }
            bool IsStatic(PropertyInfo p) => (p.GetAccessors().FirstOrDefault()?.IsStatic).GetValueOrDefault();
        }

        private void AddBindingForConstructorParametersToCommand(Type type, ICommand command, BindingFlags bindingFlags)
        {
            var ctors = type.GetConstructors(bindingFlags | BindingFlags.Instance);
            switch (ctors.Count())
            {
                case 0: // do not need constructor
                    return;
                case 1:
                    AddBindingForMethodBase(ctors.First(), command, bindingFlags);
                    break;
                default:
                    // TODO: This is probably wrong. We should have picking rules I think 
                    throw new InvalidOperationException("Currently bound types can have only one constructor");
            }

        }

        private void AddBindingForMethodBase(MethodBase methodBase, ICommand command, BindingFlags bindingFlags)
        {
            if (methodBase == null)
            {
                return;
            }
            var parameters = methodBase.GetParameters();
            foreach (var parameterInfo in parameters)
            {
                AddBinding(parameterInfo, command);
            }
        }

        private void AddBindingForStaticPropertiesToCommand(Type type, ICommand command, BindingFlags bindingFlags)
        {
            var properties = type.GetProperties(bindingFlags | BindingFlags.Static)
                              .Where(p => (p.GetAccessors().FirstOrDefault()?.IsStatic).GetValueOrDefault());
            foreach (var property in properties)
            {
                AddBinding(property, command);
            }
        }

        // TODO: Candidates for a base class
        private void AddBinding(PropertyInfo propertyInfo, ICommand command)
        {
            var (symbolCommand, symbol) = FindMatchingSymbol(propertyInfo.Name, command);
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
                    throw new InvalidOperationException("Unexpected symbol type");
            }
        }

        private void AddBinding(ParameterInfo parameterInfo, ICommand command)
        {
            var (symbolCommand, symbol) = FindMatchingSymbol(parameterInfo.Name, command);
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
                    throw new InvalidOperationException("Unexpected symbol type");
            }
        }

        private static (ICommand, ISymbolBase) FindMatchingSymbol(string name, ICommand command)
        {
            if (command.Argument.Name == name)
            {
                return (command, command.Argument);
            }
            var options = command
                          .Children
                          .OfType<Option>()
                          .Where(o => name.IsMatch(o))
                          .ToArray();
            switch (options.Length)
            {
                case 1:
                    return (command, options[0]);
                case 0:
                    break;
                default:
                    throw new ArgumentException($"Ambiguous match while trying to bind parameter {name} among: {string.Join(",", options.Select(o => o.Name))}");
            }
            return command.Parent != null
                ? FindMatchingSymbol(name, command.Parent)
                : (null, null);
        }

        void IBinder.AddBinding(Binding binding)
            => InvocationBindingSet.AddBinding(binding);

    }
}
