// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.Linq;

namespace System.CommandLine.Binding
{
    public sealed class BindingContext
    {
        private IConsole _console;

        private readonly Dictionary<Type, IValueSource> _valueSources = new Dictionary<Type, IValueSource>(); 

        public BindingContext(
            ParseResult parseResult,
            IConsole console = null)
        {
            _console = console ?? new SystemConsole();

            ParseResult = parseResult ?? throw new ArgumentNullException(nameof(parseResult));
            ServiceProvider = new ServiceProvider(this);
        }

        public ParseResult ParseResult { get; set; }

        internal IConsoleFactory ConsoleFactory { get; set; }

        internal IHelpBuilder HelpBuilder => (IHelpBuilder)ServiceProvider.GetService(typeof(IHelpBuilder));

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

        public bool TryBind(
            IValueDescriptor valueDescriptor,
            out BoundValue boundValue)
        {
            var valueSource = GetValueSource(valueDescriptor);

            if (valueSource.TryGetValue(valueDescriptor, this, out var value))
            {
                boundValue = new BoundValue(value, valueDescriptor, valueSource);
                return true;
            }
            else
            {
                boundValue = null;
                return false;
            }
        }

        private IValueSource GetValueSource(IValueDescriptor valueDescriptor)
        {
            return _valueSources.GetOrAdd(valueDescriptor.Type, type =>
            {
                if (ServiceProvider.AvailableServiceTypes.Contains(type))
                {
                    return new ServiceProviderValueSource();
                }
                else
                {
                    return new CurrentSymbolResultValueSource();
                }
            });
        }
    }

    internal class ServiceProviderValueSource : IValueSource
    {
        public bool TryGetValue(
            IValueDescriptor valueDescriptor,
            BindingContext bindingContext,
            out object value)
        {
            value = bindingContext.ServiceProvider.GetService(valueDescriptor.Type);
            return true;
        }
    }
}
