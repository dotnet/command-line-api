// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace System.CommandLine.Binding
{
    /// <summary>
    /// Creates object instances based on command line parser results, injected services, and other value sources.
    /// </summary>
    public sealed class BindingContext : IServiceProvider
    {
        private HelpBuilder? _helpBuilder;

        internal BindingContext(ParseResult parseResult)
        {
            ParseResult = parseResult;
            ServiceProvider = new ServiceProvider(this);
        }

        /// <summary>
        /// The parse result for the current invocation.
        /// </summary>
        public ParseResult ParseResult { get; }
        
        internal HelpBuilder HelpBuilder => _helpBuilder ??= (HelpBuilder)ServiceProvider.GetService(typeof(HelpBuilder))!;

        internal ServiceProvider ServiceProvider { get; }

        /// <inheritdoc />
        public object? GetService(Type serviceType) => ServiceProvider.GetService(serviceType);

        /// <summary>
        /// Adds the specified service factory to the binding context.
        /// </summary>
        /// <param name="serviceType">The type for which this service factory will provide an instance.</param>
        /// <param name="factory">A delegate that provides an instance of the specified service type.</param>
        public void AddService(Type serviceType, Func<IServiceProvider, object> factory)
        {
            ServiceProvider.AddService(serviceType, factory);
        }

        /// <summary>
        /// Adds the specified service factory to the binding context.
        /// </summary>
        /// <typeparam name="T">The type for which this service factory will provide an instance.</typeparam>
        /// <param name="factory">A delegate that provides an instance of the specified service type.</param>
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
            ParseResult parseResult,
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
                    ArgumentResult argumentResult = valueDescriptor is CliArgument argument 
                        ? parseResult.GetResult(argument) is ArgumentResult found
                            ? found
                            : new ArgumentResult(argument, parseResult.RootCommandResult.SymbolResultTree, null)
                        : new ArgumentResult(new CliArgument<string>(valueDescriptor.ValueName), parseResult.RootCommandResult.SymbolResultTree, null);

                    var parsed = ArgumentConverter.ConvertObject(
                        argumentResult,
                        valueDescriptor.ValueType,
                        value);

                    if (parsed.Result == ArgumentConversionResultType.Successful)
                    {
                        boundValue = new BoundValue(parsed.Value, valueDescriptor, valueSource);
                        return true;
                    }
                }
            }

            boundValue = default;
            return false;
        }
    }
}