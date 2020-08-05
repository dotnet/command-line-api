// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace System.CommandLine.Binding
{
    public class ModelBinder
    {
        public ModelBinder(Type modelType) : this(new AnonymousValueDescriptor(modelType))
        {
            if (modelType is null)
            {
                throw new ArgumentNullException(nameof(modelType));
            }
        }

        internal ModelBinder(IValueDescriptor valueDescriptor)
        {
            ValueDescriptor = valueDescriptor ?? throw new ArgumentNullException(nameof(valueDescriptor));

            ModelDescriptor = ModelDescriptor.FromType(valueDescriptor.ValueType);
        }

        public ModelDescriptor ModelDescriptor { get; }

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
                parent: ModelDescriptor);
            var cmpParamDescs = cmpCtorDesc.ParameterDescriptors
                .Select(GetParameterDescriptorComparands)
                .ToList();

            return ModelDescriptor.ConstructorDescriptors
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
                (desc.ValueType, desc.AllowsNull, desc.HasDefaultValue);
        }

        protected IValueDescriptor FindModelPropertyDescriptor(
            Type propertyType, string propertyName)
        {
            return ModelDescriptor.PropertyDescriptors
                .FirstOrDefault(desc =>
                    desc.ValueType == propertyType &&
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

        public object? CreateInstance(BindingContext context)
        {
            var values = GetValues(
                // No binding sources, as were are attempting to bind a value
                // for the model itself, not for its ctor args or its members.
                bindingSources: null,
                bindingContext: context,
                new[] { ValueDescriptor },
                includeMissingValues: false);

            if (values.Count == 1 &&
                ModelDescriptor.ModelType.IsAssignableFrom(values[0].ValueDescriptor.ValueType))
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
            [NotNullWhen(true)] out object? instance)
        {
            var constructorDescriptors =
                ModelDescriptor
                    .ConstructorDescriptors
                    .OrderByDescending(d => d.ParameterDescriptors.Count);

            foreach (var constructor in constructorDescriptors)
            {
                var boundConstructorArguments = GetValues(
                    ConstructorArgumentBindingSources,
                    context,
                    constructor.ParameterDescriptors,
                    true);

                if (boundConstructorArguments.Count != constructor.ParameterDescriptors.Count)
                {
                    continue;
                }

                // Found invokable constructor, invoke and return
                var values = boundConstructorArguments.Select(v => v.Value).ToArray();

                try
                {
                    var fromModelBinder = constructor.Invoke(values);

                    UpdateInstance(fromModelBinder, context);

                    instance = fromModelBinder;

                    return true;
                }
                catch
                {
                    instance = null;
                    return false;
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
                ModelDescriptor.PropertyDescriptors,
                includeMissingValues: false);

            foreach (var boundValue in boundValues)
            {
                ((PropertyDescriptor)boundValue.ValueDescriptor).SetValue(instance, boundValue.Value);
            }
        }

        private IReadOnlyList<BoundValue> GetValues(
            IDictionary<IValueDescriptor, IValueSource>? bindingSources,
            BindingContext bindingContext,
            IReadOnlyList<IValueDescriptor> valueDescriptors,
            bool includeMissingValues)
        {
            var values = new List<BoundValue>();

            for (var index = 0; index < valueDescriptors.Count; index++)
            {
                var valueDescriptor = valueDescriptors[index];

                var valueSource = GetValueSource(bindingSources, bindingContext, valueDescriptor);

                BoundValue? boundValue = GetBoundValue(valueSource, bindingContext, valueDescriptor);

                if (boundValue is null)
                {
                    if (includeMissingValues)
                    {
                        if (valueDescriptor is ParameterDescriptor parameterDescriptor &&
                            parameterDescriptor.Parent is ConstructorDescriptor constructorDescriptor)
                        {
                            if (parameterDescriptor.HasDefaultValue)
                                boundValue = BoundValue.DefaultForValueDescriptor(parameterDescriptor);
                            else if (parameterDescriptor.AllowsNull &&
                                ShouldPassNullToConstructor(constructorDescriptor.Parent, constructorDescriptor))
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

        internal static BoundValue? GetBoundValue(IValueSource valueSource, BindingContext bindingContext,
                                                  IValueDescriptor valueDescriptor)
        {
            BoundValue? boundValue;
            if (!bindingContext.TryBindToScalarValue(
                    valueDescriptor,
                    valueSource,
                    out boundValue) && valueDescriptor.HasDefaultValue)
            {
                boundValue = BoundValue.DefaultForValueDescriptor(valueDescriptor);
            }

            return boundValue;
        }

        private IValueSource GetValueSource(
            IDictionary<IValueDescriptor, IValueSource>? bindingSources,
            BindingContext bindingContext,
            IValueDescriptor valueDescriptor)
        {
            if (!(bindingSources is null) &&
                bindingSources.TryGetValue(valueDescriptor, out IValueSource? valueSource))
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

            return new MissingValueSource();
        }

        public override string ToString() =>
            $"{ModelDescriptor.ModelType.Name}";

        private static bool ShouldPassNullToConstructor(ModelDescriptor modelDescriptor,
            ConstructorDescriptor? ctor = null)
        {
            if (!(ctor is null))
            {
                return ctor.ParameterDescriptors.All(d => d.AllowsNull);
            }

            return !modelDescriptor.ModelType.IsNullable();
        }

        private class AnonymousValueDescriptor : IValueDescriptor
        {
            public Type ValueType { get; }

            public AnonymousValueDescriptor(Type modelType)
            {
                ValueType = modelType;
            }

            public string ValueName => "";

            public bool HasDefaultValue => false;

            public object? GetDefaultValue() => null;

            public override string ToString() => $"{ValueType}";
        }
    }
}
