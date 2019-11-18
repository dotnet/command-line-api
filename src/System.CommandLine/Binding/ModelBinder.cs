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

        public bool EnforceExplicitBinding { get; set; }

        internal Dictionary<IValueDescriptor, IValueSource> ConstructorArgumentBindingSources { get; } =
            new Dictionary<IValueDescriptor, IValueSource>();

        internal Dictionary<IValueDescriptor, IValueSource> MemberBindingSources { get; } =
            new Dictionary<IValueDescriptor, IValueSource>();

        protected ConstructorDescriptor FindModelConstructorDescriptor(
            ConstructorInfo constructorInfo)
        {
            var cmpCtorDesc = new ConstructorDescriptor(constructorInfo,
                // Parent does not matter for comparison and can be invalid.
                parent: _modelDescriptor);
            var cmpParamDescs = cmpCtorDesc.ParameterDescriptors
                .Select(GetParameterDescriptorComparands)
                .ToList();

            return _modelDescriptor.ConstructorDescriptors
                .FirstOrDefault(matchCtorDesc =>
                {
                    if (matchCtorDesc.Parent.ModelType != constructorInfo.DeclaringType)
                        return false;
                    return matchCtorDesc.ParameterDescriptors
                        .Select(GetParameterDescriptorComparands)
                        .SequenceEqual(cmpParamDescs);
                });

            // Name matching is not necessary for overload descisions.
            static (Type paramType, bool allowNull, bool hasDefaultValue)
                GetParameterDescriptorComparands(ParameterDescriptor desc) =>
                (desc.Type, desc.AllowsNull, desc.HasDefaultValue);
        }

        protected IValueDescriptor FindModelPropertyDescriptor(
            Type propertyType, string propertyName)
        {
            return _modelDescriptor.PropertyDescriptors
                .FirstOrDefault(desc =>
                    desc.Type == propertyType &&
                    string.Equals(desc.ValueName, propertyName, StringComparison.Ordinal)
                    );
        }

        public void BindConstructorArgumentFromValue(ParameterInfo parameter,
            IValueDescriptor valueDescriptor)
        {
            if (!(parameter.Member is ConstructorInfo constructor))
                throw new ArgumentException(paramName: nameof(parameter),
                    message: "Parameter must be declared on a constructor.");

            var ctorDesc = FindModelConstructorDescriptor(constructor);
            if (ctorDesc is null)
                throw new ArgumentException(paramName: nameof(parameter),
                    message: "Parameter is not described by any of the model constructor descriptors.");
            
            var paramDesc = ctorDesc.ParameterDescriptors[parameter.Position];
            ConstructorArgumentBindingSources[paramDesc] =
                new SpecificSymbolValueSource(valueDescriptor);
        }

        public void BindMemberFromValue(PropertyInfo property, 
            IValueDescriptor valueDescriptor)
        {
            var propertyDescriptor = FindModelPropertyDescriptor(
                property.PropertyType, property.Name);
            if (propertyDescriptor is null)
                throw new ArgumentException(paramName: nameof(property),
                    message: "Property is not described by any of the model property descriptors.");

            MemberBindingSources[propertyDescriptor] =
                new SpecificSymbolValueSource(valueDescriptor);
        }

        public object CreateInstance(BindingContext context)
        {
            var values = GetValues(
                // No binding sources, as were are attempting to bind a value
                // for the model itself, not for its ctor args or its members.
                bindingSources: null, 
                bindingContext: context, 
                new[] { ValueDescriptor }, 
                includeMissingValues: false);

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
            var constructorDescriptors = _modelDescriptor.ConstructorDescriptors
                // Find constructors with most non-optional parameters first
                .OrderByDescending(d => d.ParameterDescriptors.Count(p => !p.HasDefaultValue));

            // Attempt first to bind all values, then attempt to fill default values
            foreach (bool includeMissingValues in new[] { false, true })
            {
                foreach (var constructor in constructorDescriptors)
                {
                    var boundConstructorArguments = GetValues(
                        ConstructorArgumentBindingSources,
                        context, constructor.ParameterDescriptors,
                        includeMissingValues);
                    if (boundConstructorArguments.Count != constructor.ParameterDescriptors.Count)
                        continue;
                    
                    // Found invocable constructor, invoke and return
                    var values = boundConstructorArguments.Select(v => v.Value).ToArray();
                    var fromModelBinder = constructor.Invoke(values);

                    UpdateInstance(fromModelBinder, context);

                    instance = fromModelBinder;
                    return true;
                }
            }

            instance = null;
            return false;
        }

        public void UpdateInstance<T>(T instance, BindingContext bindingContext)
        {
            var boundValues = GetValues(
                MemberBindingSources,
                bindingContext,
                _modelDescriptor.PropertyDescriptors,
                includeMissingValues: false);

            foreach (var boundValue in boundValues)
            {
                ((PropertyDescriptor)boundValue.ValueDescriptor).SetValue(instance, boundValue.Value);
            }
        }

        private IReadOnlyList<BoundValue> GetValues(
            IDictionary<IValueDescriptor, IValueSource> bindingSources,
            BindingContext bindingContext,
            IReadOnlyList<IValueDescriptor> valueDescriptors,
            bool includeMissingValues)
        {
            var values = new List<BoundValue>();

            for (var index = 0; index < valueDescriptors.Count; index++)
            {
                var valueDescriptor = valueDescriptors[index];

                var valueSource = GetValueSource(bindingSources, bindingContext, valueDescriptor);

                BoundValue boundValue;
                if (valueSource is null)
                {
                    // If there is no source to bind from, no value can be bound.
                    boundValue = null;
                }
                else if (!bindingContext.TryBindToScalarValue(
                        valueDescriptor,
                        valueSource,
                        out boundValue) && valueDescriptor.HasDefaultValue)
                {
                    boundValue = BoundValue.DefaultForValueDescriptor(valueDescriptor);
                }

                if (boundValue == null)
                {
                    if (includeMissingValues)
                    {
                        if (valueDescriptor is ParameterDescriptor parameterDescriptor &&
                            parameterDescriptor.Parent is ConstructorDescriptor constructorDescriptor &&
                            ShouldPassNullToConstructor(constructorDescriptor.Parent, constructorDescriptor))
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
            IDictionary<IValueDescriptor, IValueSource> bindingSources,
            BindingContext bindingContext,
            IValueDescriptor valueDescriptor)
        {
            if (!(bindingSources is null) &&
                bindingSources.TryGetValue(valueDescriptor, out var valueSource))
            {
                return valueSource;
            }

            if (bindingContext.TryGetValueSource(valueDescriptor, out valueSource))
            {
                return valueSource;
            }

            if (!EnforceExplicitBinding)
            {
                // Return a value source that will match from the parseResult
                // by name and type (or a possible conversion)
                return new ParseResultMatchingValueSource();
            }

            return null;
        }

        public override string ToString() =>
            $"{_modelDescriptor.ModelType.Name}";

        private bool ShouldPassNullToConstructor(ModelDescriptor modelDescriptor,
            ConstructorDescriptor ctor = null)
        {
            if (!(ctor is null))
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

            public string ValueName => null;

            public bool HasDefaultValue => false;

            public object GetDefaultValue() => null;

            public override string ToString() => $"{Type}";
        }
    }
}
