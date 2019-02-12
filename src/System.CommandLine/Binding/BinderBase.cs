// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine.Binding
{
    public abstract class BinderBase
    {
        protected Dictionary<Type, IValueSource> ValueSources { get; }
            = new Dictionary<Type, IValueSource>();

        protected Dictionary<(Type valueSourceType, string valueSourceName), IValueSource> NamedValueSources { get; }
            = new Dictionary<(Type valueSourceType, string valueSourceName), IValueSource>();

        protected IReadOnlyCollection<BoundValue> GetValues(
            BindingContext bindingContext,
            IReadOnlyList<IValueDescriptor> valueDescriptors,
            bool includeMissingValues = true)
        {
            var values = new List<BoundValue>();

            for (var index = 0; index < valueDescriptors.Count; index++)
            {
                var valueDescriptor = valueDescriptors[index];
                var valueSource = GetValueSource(bindingContext, valueDescriptor);

                if (bindingContext.TryBind(
                    valueDescriptor,
                    valueSource,
                    out var value))
                {
                    values.Add(value);
                }
                else if (includeMissingValues)
                {
                    values.Add(BoundValue.DefaultForType(valueDescriptor));
                }
            }

            return values;
        }

        private IValueSource GetValueSource(
            BindingContext bindingContext,
            IValueDescriptor valueDescriptor)
        {
            if (NamedValueSources.TryGetValue(
                (valueDescriptor.Type, valueDescriptor.Name),
                out var valueSource))
            {
                return valueSource;
            }

            return ValueSources.GetOrAdd(
                valueDescriptor.Type,
                type => CreateDefaultValueSource(bindingContext, type));
        }

        private static IValueSource CreateDefaultValueSource(BindingContext bindingContext, Type type)
        {
            if (bindingContext.ServiceProvider.AvailableServiceTypes.Contains(type))
            {
                return new ServiceProviderValueSource();
            }
            else
            {
                return new CurrentSymbolResultValueSource();
            }
        }
    }
}
