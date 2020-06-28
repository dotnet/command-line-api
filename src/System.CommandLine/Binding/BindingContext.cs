// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

#nullable enable

namespace System.CommandLine.Binding
{
    public sealed class BindingContext
    {
        private IConsole _console;
        private readonly Dictionary<Type, ModelBinder> _modelBindersByValueDescriptor = new Dictionary<Type, ModelBinder>();
        private readonly Dictionary<MethodBase, MethodBinder> _methodBindersByValueDescriptor = new Dictionary<MethodBase, MethodBinder>();

        public BindingContext(
            ParseResult parseResult,
            IConsole? console = default)
        {
            _console = console ?? new SystemConsole();

            ParseResult = parseResult ?? throw new ArgumentNullException(nameof(parseResult));
            ServiceProvider = new ServiceProvider(this);
        }

        public ParseResult ParseResult { get; set; }

        internal IConsoleFactory? ConsoleFactory { get; set; }

        internal IHelpBuilder HelpBuilder => (IHelpBuilder)ServiceProvider.GetService(typeof(IHelpBuilder))!;

        public IConsole Console
        {
            get
            {
                if (ConsoleFactory != null)
                {
                    var consoleFactory = ConsoleFactory;
                    ConsoleFactory = null;
                    _console = consoleFactory.CreateConsole(this);
                }

                return _console;
            }
        }

        internal ServiceProvider ServiceProvider { get; }

        public void AddModelBinder(ModelBinder binder) => 
            _modelBindersByValueDescriptor.Add(binder.ValueDescriptor.ValueType, binder);

        public ModelBinder GetModelBinder(IValueDescriptor valueDescriptor)
        {
            if (_modelBindersByValueDescriptor.TryGetValue(valueDescriptor.ValueType, out ModelBinder binder))
            {
                return binder;
            }
            return new ModelBinder(valueDescriptor);
        }

        public void AddMethodBinder(MethodBinder binder) =>
            _methodBindersByValueDescriptor.Add(binder.MethodDescriptor.MethodInfo, binder);

        public MethodBinder GetMethodBinder(MethodInfo methodInfo)
        {
            if (_methodBindersByValueDescriptor.TryGetValue(methodInfo, out MethodBinder binder))
            {
                return binder;
            }
            return new MethodBinder(methodInfo);
        }

        public void AddService(Type serviceType, Func<IServiceProvider, object> factory)
        {
            ServiceProvider.AddService(serviceType, factory);
        }
        
        public void AddService<T>(Func<IServiceProvider, T> factory)
        {
            if (factory is null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            ServiceProvider.AddService(typeof(T), s => factory(s));
        }

        internal bool TryGetValueSource(
            IValueDescriptor valueDescriptor,
            [MaybeNullWhen(false)] out IValueSource valueSource)
        {
            if (ServiceProvider.AvailableServiceTypes.Contains(valueDescriptor.ValueType))
            {
                valueSource = new ServiceProviderValueSource();
                return true;
            }

            valueSource = default!;
            return false;
        }

        internal bool TryBindToScalarValue(
            IValueDescriptor valueDescriptor,
            IValueSource valueSource,
            out BoundValue? boundValue)
        {
            if (valueSource.TryGetValue(valueDescriptor, this, out var value))
            {
                if (value is null || valueDescriptor.ValueType.IsInstanceOfType(value))
                {
                    boundValue = new BoundValue(value, valueDescriptor, valueSource);
                    return true;
                }
                else
                {
                    var parsed = ArgumentConverter.ConvertObject(
                        valueDescriptor as IArgument ?? new Argument(valueDescriptor.ValueName), 
                        valueDescriptor.ValueType, 
                        value);

                    if (parsed is SuccessfulArgumentConversionResult successful)
                    {
                        boundValue = new BoundValue(successful.Value, valueDescriptor, valueSource);
                        return true;
                    }
                }
            }

            boundValue = default;
            return false;
        }
    }
}
