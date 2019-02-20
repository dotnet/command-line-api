// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.Threading;

namespace System.CommandLine.Invocation
{
    internal class ServiceProvider : IServiceProvider
    {
        private readonly BindingContext _bindingContext;

        private readonly Dictionary<Type, Func<object>> _services;

        public ServiceProvider(BindingContext bindingContext)
        {
            _bindingContext = bindingContext ?? throw new ArgumentNullException(nameof(bindingContext));

            _services = new Dictionary<Type, Func<object>>
                        {
                            [typeof(ParseResult)] = () => _bindingContext.ParseResult,
                            [typeof(IConsole)] = () => _bindingContext.Console,
                            [typeof(CancellationToken)] = () => CancellationToken.None,
                            [typeof(IHelpBuilder)] = () => _bindingContext.ParseResult.Parser.Configuration.HelpBuilderFactory.CreateHelpBuilder(_bindingContext),
                            [typeof(BindingContext)] = () => this
                        };
        }

        public void AddService<T>(Func<T> factory) => _services[typeof(T)] = () => factory();

        public void AddService(Type serviceType, Func<object> factory) => _services[serviceType] = factory;

        public IReadOnlyCollection<Type> AvailableServiceTypes => _services.Keys;

        public object GetService(Type serviceType)
        {
            if (_services.TryGetValue(serviceType, out var factory))
            {
                return factory();
            }

            return null;
        }
    }
}
