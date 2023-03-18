// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;

namespace System.CommandLine.Invocation
{
    internal sealed class ServiceProvider : IServiceProvider
    {
        private readonly Dictionary<Type, Func<IServiceProvider, object?>> _services;

        internal ServiceProvider(BindingContext bindingContext)
        {
            _services = new Dictionary<Type, Func<IServiceProvider, object?>>
                        {
                            [typeof(ParseResult)] = _ => bindingContext.ParseResult,
                            [typeof(BindingContext)] = _ => bindingContext
                        };
        }

        public void AddService<T>(Func<IServiceProvider, T> factory) => _services[typeof(T)] = p => factory(p)!;

        public void AddService(Type serviceType, Func<IServiceProvider, object?> factory) => _services[serviceType] = factory;

        public IReadOnlyCollection<Type> AvailableServiceTypes => _services.Keys;

        public object? GetService(Type serviceType)
        {
            if (_services.TryGetValue(serviceType, out var factory))
            {
                return factory(this);
            }

            return null;
        }
    }
}
