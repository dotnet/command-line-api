// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace System.CommandLine.Binding
{
    public class ModelBinder
    {
        private readonly ConstructorDescriptor _constructorDescriptor;

        private readonly ModelDescriptor _modelDescriptor;

        public ModelBinder(Type modelType) : this(new AnonymousValueDescriptor(modelType))
        {
            if (modelType == null)
            {
                throw new ArgumentNullException(nameof(modelType));
            }
        }

        internal ModelBinder(IValueDescriptor valueDescriptor)
        {
            ValueDescriptor = valueDescriptor ?? throw new ArgumentNullException(nameof(valueDescriptor));

            _modelDescriptor = ModelDescriptor.FromType(valueDescriptor.Type);

            if (_modelDescriptor.ConstructorDescriptors.Count == 1)
            {
                _constructorDescriptor = _modelDescriptor.ConstructorDescriptors[0];
            }
        }

        public IValueDescriptor ValueDescriptor { get; }

        internal Dictionary<(Type valueSourceType, string valueSourceName), IValueSource> ValueSources { get; }
            = new Dictionary<(Type valueSourceType, string valueSourceName), IValueSource>();

        public IReadOnlyCollection<BoundValue> GetConstructorArguments(BindingContext context)
        {
            if (_constructorDescriptor != null)
            {
                return GetValues(context, _constructorDescriptor.ParameterDescriptors);
            }
            else
            {
                return Array.Empty<BoundValue>();
            }
        }

        public void BindMemberFromValue(PropertyInfo property, Option option)
        {
            ValueSources.Add(
                (property.PropertyType, property.Name),
                new SpecificSymbolValueSource(option));
        }

        public void BindMemberFromValue(PropertyInfo property, Command command)
        {
            ValueSources.Add(
                (property.PropertyType, property.Name),
                new SpecificSymbolValueSource(command));
        }

        public object CreateInstance(BindingContext context)
        {
            if (context.ServiceProvider.AvailableServiceTypes.Contains(_modelDescriptor.ModelType))
            {
                return GetOrCreateInstance(context);
            }

            var boundConstructorArguments = GetConstructorArguments(context);

            var values = boundConstructorArguments.Select(v => v.Value).ToArray();

            object instance;
            if (_constructorDescriptor?.Invoke(values) != null)
            {
                instance = _constructorDescriptor.Invoke(values);
                UpdateInstance(instance, context);
            }
            else
            {
                instance = GetOrCreateInstance(context);
            }

            return instance;
        }

        internal object GetOrCreateInstance(BindingContext context) =>
            GetValues(context,
                      new[] { ValueDescriptor })
                .SingleOrDefault()?.Value;

        public void UpdateInstance<T>(T instance, BindingContext bindingContext)
        {
            SetProperties(bindingContext, instance);
        }

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

                if (!TryBind(
                        bindingContext,
                        valueDescriptor,
                        valueSource,
                        out BoundValue value))
                {
                    if (includeMissingValues)
                    {
                        value = BoundValue.DefaultForType(valueDescriptor);
                    }
                }

                if (value != null)
                {
                    values.Add(value);
                }
            }

            return values;
        }

        internal bool TryBind(
            BindingContext bindingContext,
            IValueDescriptor valueDescriptor,
            IValueSource valueSource,
            out BoundValue boundValue)
        {
            if (valueSource.TryGetValue(valueDescriptor, bindingContext, out var value))
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

        private IValueSource GetValueSource(
            BindingContext bindingContext,
            IValueDescriptor valueDescriptor)
        {
            IValueSource valueSource;

            if (valueDescriptor?.Name != null)
            {
                if (ValueSources.TryGetValue(
                    (valueDescriptor.Type, valueDescriptor.Name),
                    out valueSource))
                {
                    return valueSource;
                }

                if (bindingContext.NamedValueSources.TryGetValue(
                    (valueDescriptor.Type, valueDescriptor.Name),
                    out valueSource))
                {
                    return valueSource;
                }
            }

            return bindingContext.ValueSources.GetOrAdd(
                valueDescriptor.Type,
                type => CreateDefaultValueSource(bindingContext, type));
        }

        private static IValueSource CreateDefaultValueSource(
            BindingContext bindingContext, 
            Type type)
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

        private void SetProperties(
            BindingContext context,
            object instance)
        {
            var propertyValues = GetValues(
                context,
                _modelDescriptor.PropertyDescriptors,
                false);

            foreach (var boundValue in propertyValues)
            {
                ((PropertyDescriptor)boundValue.ValueDescriptor).SetValue(instance, boundValue.Value);
            }
        }

        private class AnonymousValueDescriptor : IValueDescriptor
        {
            public Type Type { get; }

            public AnonymousValueDescriptor(Type modelType)
            {
                Type = modelType;
            }

            public string Name => null;

            public bool HasDefaultValue => false;

            public object GetDefaultValue() => null;
        }
    }
}
