// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace System.CommandLine.Binding
{
    public class ModelBinder
    {
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
        }

        public IValueDescriptor ValueDescriptor { get; }

        internal Dictionary<(Type valueSourceType, string valueSourceName), IValueSource> NamedValueSources { get; }
            = new Dictionary<(Type valueSourceType, string valueSourceName), IValueSource>();

        public void BindMemberFromValue(PropertyInfo property, Option option)
        {
            NamedValueSources.Add(
                (property.PropertyType, property.Name),
                new OptionValueSource(option));
        }

        public object CreateInstance(BindingContext context)
        {
            var values = GetValues(context, new[] { ValueDescriptor }, false);

            if (values.Count == 1 &&
                _modelDescriptor.ModelType.IsAssignableFrom(values[0].ValueDescriptor.Type))
            {
                return values[0].Value;
            }

            if (TryDefaultConstructorAndPropertiesStrategy(context, out var fromCtor))
            {
                return fromCtor;
            }

            return values.SingleOrDefault()?.Value;
        }

        private bool TryDefaultConstructorAndPropertiesStrategy(
            BindingContext context,
            out object instance)
        {
            var targetConstructorDescriptor = _modelDescriptor.TargetConstructor;

            if (targetConstructorDescriptor == null)
            {
                instance = null;
                return false;
            }

            var boundConstructorArguments = GetValues(
                context, 
                targetConstructorDescriptor.ParameterDescriptors,
                true);

            if (boundConstructorArguments.Count != targetConstructorDescriptor.ParameterDescriptors.Count)
            {
                instance = null;
                return false;
            }

            var values = boundConstructorArguments.Select(v => v.Value).ToArray();
            var fromModelBinder = targetConstructorDescriptor.Invoke(values);

            UpdateInstance(fromModelBinder, context);

            instance = fromModelBinder;

            return true;
        }

        public void UpdateInstance<T>(T instance, BindingContext bindingContext)
        {
            var boundValues = GetValues(
                bindingContext,
                _modelDescriptor.PropertyDescriptors,
                false);

            foreach (var boundValue in boundValues)
            {
                ((PropertyDescriptor)boundValue.ValueDescriptor).SetValue(instance, boundValue.Value);
            }
        }

        private IReadOnlyList<BoundValue> GetValues(
            BindingContext bindingContext,
            IReadOnlyList<IValueDescriptor> valueDescriptors,
            bool includeMissingValues)
        {
            var values = new List<BoundValue>();

            for (var index = 0; index < valueDescriptors.Count; index++)
            {
                var valueDescriptor = valueDescriptors[index];

                var valueSource = GetValueSource(bindingContext, valueDescriptor);

                if (!bindingContext.TryBindToScalarValue(
                        valueDescriptor,
                        valueSource,
                        out BoundValue boundValue) && valueDescriptor.HasDefaultValue)
                {
                    boundValue = BoundValue.DefaultForValueDescriptor(valueDescriptor);
                }

                if (boundValue == null)
                {
                    if (includeMissingValues)
                    {
                        if (valueDescriptor is ParameterDescriptor parameterDescriptor &&
                            parameterDescriptor.Parent?.Parent is ModelDescriptor modelDescriptor && 
                            ShouldPassNullToConstructor(modelDescriptor))
                        {
                            boundValue = BoundValue.DefaultForType(valueDescriptor);
                        }
                    }
                }

                if (boundValue != null)
                {
                    values.Add(boundValue);
                }
            }

            return values;
        }

        private IValueSource GetValueSource(
            BindingContext bindingContext,
            IValueDescriptor valueDescriptor)
        {
            var type = valueDescriptor.Type;
            var name = valueDescriptor.Name;

            if (NamedValueSources.TryGetValue(
                (type, name),
                out var valueSource))
            {
                return valueSource;
            }

            if (bindingContext.TryGetValueSource(valueDescriptor, out valueSource))
            {
                return valueSource;
            }

            return new CurrentSymbolResultValueSource();
        }

        public override string ToString() =>
            $"{_modelDescriptor.ModelType.Name}";

        private bool ShouldPassNullToConstructor(ModelDescriptor modelDescriptor)
        {
            if (modelDescriptor.TargetConstructor is ConstructorDescriptor ctor)
            {
                return ctor.ParameterDescriptors.All(d => d.AllowsNull);
            }

            return !modelDescriptor.ModelType.IsNullable();
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

            public override string ToString() => $"{Type}";
        }
    }
}
