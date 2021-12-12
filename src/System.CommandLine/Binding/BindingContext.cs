// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

#nullable enable

namespace System.CommandLine.Binding
{
    /// <summary>
    /// Creates object instances based on command line parser results, injected services, and other value sources.
    /// </summary>
    public sealed class BindingContext : IServiceProvider
    {
        private IConsole _console;
        private HelpBuilder? _helpBuilder;

        /// <param name="parseResult">The parse result used for binding to command line input.</param>
        /// <param name="console">A console instance used for writing output.</param>
        public BindingContext(
            ParseResult parseResult,
            IConsole? console = default)
        {
            _console = console ?? new SystemConsole();

            ParseResult = parseResult ?? throw new ArgumentNullException(nameof(parseResult));
            ServiceProvider = new ServiceProvider(this);
        }

        /// <summary>
        /// The parse result for the current invocation.
        /// </summary>
        public ParseResult ParseResult { get; set; }

        internal IConsoleFactory? ConsoleFactory { get; set; }

        internal HelpBuilder HelpBuilder => _helpBuilder ??= (HelpBuilder)ServiceProvider.GetService(typeof(HelpBuilder))!;

        /// <summary>
        /// The console to which output should be written during the current invocation.
        /// </summary>
        public IConsole Console
        {
            get
            {
                if (ConsoleFactory is not null)
                {
                    var consoleFactory = ConsoleFactory;
                    ConsoleFactory = null;
                    _console = consoleFactory.CreateConsole(this);
                }

                return _console;
            }
        }

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
            LocalizationResources localizationResources,
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
                        value,
                        localizationResources);

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