// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace System.CommandLine.Binding
{
    public class ReflectionBinder : IBinder
    {
        public ReflectionBinder(Type type)
            => _type = type;

        internal const BindingFlags CommonBindingFlags =
            BindingFlags.IgnoreCase
            | BindingFlags.Public
            | BindingFlags.Instance;

        private object _explicitlySetTarget;

        private readonly Type _type;

        private MethodInfo _handlerMethodInfo;

        private ParameterCollection _handlerParameterCollection;
        private ParameterCollection _constructorParameterCollection;

        private readonly BindingSet _constructorBindingSet = new BindingSet();
        private readonly BindingSet _handlerBindingSet = new BindingSet();

        private bool _isBoundToCommand;

        internal void SetInvocationMethod(MethodInfo methodInfo)
            => _handlerMethodInfo = methodInfo;

        public void AddBinding(ParameterInfo parameterInfo, Option option)
            => AddBinding(parameterInfo, SymbolBindingSide.Create(option));

        public void AddBinding(ParameterInfo parameterInfo, Argument argument)
            => AddBinding(parameterInfo, SymbolBindingSide.Create(argument));

        public void AddBinding<T>(ParameterInfo parameterInfo, Func<T> valueFunc)
            => AddBinding(parameterInfo, ValueBindingSide.Create(valueFunc));

        private void AddBinding(ParameterInfo parameterInfo, BindingSide parserBindingSide)
        {
            switch (parameterInfo.Member)
            {
                case MethodInfo methodInfo:
                    _handlerMethodInfo = _handlerMethodInfo
                                         ?? methodInfo;
                    _handlerParameterCollection = _handlerParameterCollection
                                                  ?? new ParameterCollection(methodInfo);
                    _handlerBindingSet.AddBinding(ParameterBindingSide.Create(
                                                      parameterInfo, _handlerParameterCollection), parserBindingSide);
                    return;
                case ConstructorInfo constructorInfo:
                    _constructorParameterCollection = _constructorParameterCollection
                                                      ?? new ParameterCollection(constructorInfo);
                    _constructorBindingSet.AddBinding(ParameterBindingSide.Create(parameterInfo, _constructorParameterCollection), parserBindingSide);
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
                _constructorBindingSet.AddBinding(propertyBindingSide, parserBindingSide);
            }
            else
            {
                _handlerBindingSet.AddBinding(propertyBindingSide, parserBindingSide);
            }
        }

        public void AddBindings(Type type, MethodInfo methodInfo, ICommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            type = type ?? methodInfo.DeclaringType;

            // bind in same order as invocation. not sure this matters
            AddBindingForConstructorParametersToCommand(type, command);
            AddBindingForPropertiesToCommand(type, command);
            AddBindingForParametersToCommand(methodInfo, command);
            AddBindingForServiceParameters();

            _isBoundToCommand = true;
        }

        internal void AddBindingsIfNeeded(ICommand command)
        {
            // I am not crazy about using a boolean. It won't play nice with mixed 
            // mode binding (manual and automatic)
            if (!_isBoundToCommand)
            {
                AddBindings(_type, _handlerMethodInfo, command);
            }
        }

        public void SetTarget(object target)
            => _explicitlySetTarget = target;

        private object[] GetNullHandledConstructorArguments()
            => HandleNullArguments(_constructorParameterCollection?.GetArguments());

        private object[] GetNullHandledInvocationArguments()
            => HandleNullArguments(_handlerParameterCollection?.GetArguments());

        private void BindConstructor(BindingContext context)
            => _constructorBindingSet.Bind(context, null);

        private void BindProperties(BindingContext context, object target)
            => _handlerBindingSet.Bind(context, target);

        public object GetTarget(BindingContext context)
        {
            AddBindingsIfNeeded(context.ParseResult.CommandResult.Command);

            // Allow for the possibility that constructor binding explicitly sets the target. 
            BindConstructor(context);

            object target;
            if (_explicitlySetTarget != null)
            {
                target = _explicitlySetTarget;
            }
            else
            {
                target = Activator.CreateInstance(_type,
                                                  GetNullHandledConstructorArguments());
            }

            BindProperties(context, target);

            return target;
        }

        public object[] GetInvocationArguments(BindingContext context)
        {
            BindConstructor(context);

            // Because of the relationship between targets and invocation arguments, the target
            // needs to be created and the invocation bound in GetTarget. 
            // Invocation arguments can depend on properties, which are set by binding the invocation. 
            GetTarget(context);

            return GetNullHandledInvocationArguments();
        }

        public object InvokeAsync(InvocationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var target = GetTarget(context.BindingContext);
            // Invocation bind is done during Target construction (to allow dependency on properties)
            var value = _handlerMethodInfo.Invoke(target, GetNullHandledInvocationArguments());

            return CommandHandler.GetResultCodeAsync(value, context);
        }

        public IEnumerable<ParameterInfo> GetUnboundParameters(
            MethodInfo methodInfo,
            BindingSet bindingSet,
            bool considerParameterDefaults = false)
        {
            if (methodInfo == null)
            {
                return new List<ParameterInfo>();
            }

            return methodInfo.GetParameters()
                             .Where(p =>
                             {
                                 if (considerParameterDefaults && p.HasDefaultValue)
                                 {
                                     return true;
                                 }

                                 if (bindingSet.FindTargetBinding<ParameterBindingSide>(pbs => pbs.ParameterInfo == p)
                                               .None())
                                 {
                                     return true;
                                 }

                                 return false;
                             });
        }

        private static object[] HandleNullArguments(object[] arguments)
            => arguments
               ?? Array.Empty<object>();

        // If a corresponding symbol isn't fuond, that's fine Invocation should use default
        // Add a way to include an explicit default from parameter info when there is no option
        // Add a "strict" method test that warns on missing symbols. Levels including ignore static properties
        private void AddBindingForPropertyParameters(Type type, IEnumerable<ParameterInfo> unboundParameters)
        {
            foreach (var parameterInfo in unboundParameters)
            {
                const BindingFlags bindingFlags = CommonBindingFlags;

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

        private void AddBindingForServiceParameters()
        {
            var unboundParameters = GetUnboundParameters(_handlerMethodInfo, _handlerBindingSet);

            // TODO: (AddBindingForServiceParameters) delete this 
            var bindable = unboundParameters
                .Where(p => new[]
                            {
                                typeof(IConsole),
                                typeof(IHelpBuilder),
                                typeof(InvocationContext),
                                typeof(ParseResult),
                                typeof(CancellationToken)
                            }.Contains(p.ParameterType));

            foreach (var parameterInfo in bindable)
            {
                AddBinding(parameterInfo, ServiceBindingSide.Create(parameterInfo.ParameterType));
            }
        }

        private void AddBindingForParametersToCommand(MethodInfo methodInfo, ICommand command)
        {
            AddBindingForMethodBase(methodInfo, command);
        }

        private void AddBindingForPropertiesToCommand(Type type, ICommand command)
        {
            PropertyInfo[] propertyInfos = type.GetProperties(CommonBindingFlags);
            var properties = propertyInfos
                .Where(p => !IsStatic(p) && p.CanWrite);
            foreach (var property in properties)
            {
                AddBinding(property, command);
            }

            bool IsStatic(PropertyInfo p) => (p.GetAccessors().FirstOrDefault()?.IsStatic).GetValueOrDefault();
        }

        private void AddBindingForConstructorParametersToCommand(Type type, ICommand command)
        {
            var ctors = type.GetConstructors(CommonBindingFlags);

            switch (ctors.Length)
            {
                case 0: // do not need constructor
                    return;
                case 1:
                    AddBindingForMethodBase(ctors.First(), command);
                    break;
                default:
                    // TODO: This is probably wrong. We should have picking rules I think 
                    throw new InvalidOperationException("Currently bound types can have only one constructor");
            }
        }

        private void AddBindingForMethodBase(MethodBase methodBase, ICommand command)
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
                    throw new InvalidOperationException("Unexpected symbol type");
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
                    throw new InvalidOperationException("Unexpected symbol type");
            }
        }

        private static IValueDescriptor FindMatchingSymbol(string name, ICommand command)
        {
            if (command.Argument.Name == name)
            {
                return command.Argument;
            }

            var options = command
                          .Children
                          .OfType<IOption>()
                          .Where(name.IsMatch)
                          .ToArray();

            switch (options.Length)
            {
                case 1:
                    return options[0];
                case 0:
                    break;
                default:
                    throw new ArgumentException($"Ambiguous match while trying to bind parameter {name} among: {string.Join(",", options.Select(o => o.Name))}");
            }

            return command.Parent != null
                       ? FindMatchingSymbol(name, command.Parent)
                       : null;
        }
    }
}
