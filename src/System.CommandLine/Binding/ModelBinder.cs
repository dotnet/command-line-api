using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace System.CommandLine.Binding
{
    public class ModelBinder
    {
        public ModelBinder(Type modelType)
            : this(new AnonymousValueDescriptor(modelType))
            => _ = modelType ?? throw new ArgumentNullException(nameof(modelType));

        internal ModelBinder(IValueDescriptor valueDescriptor)
        {
            ValueDescriptor = valueDescriptor ?? throw new ArgumentNullException(nameof(valueDescriptor));
            ModelDescriptor = ModelDescriptor.FromType(valueDescriptor.ValueType);
        }

        public IValueDescriptor ValueDescriptor { get; }
        public ModelDescriptor ModelDescriptor { get; }
        public bool EnforceExplicitBinding { get; set; }

        internal Dictionary<IValueDescriptor, IValueSource> ConstructorArgumentBindingSources { get; } =
            new Dictionary<IValueDescriptor, IValueSource>();

        internal Dictionary<IValueDescriptor, IValueSource> MemberBindingSources { get; } =
            new Dictionary<IValueDescriptor, IValueSource>();

        // Consider deprecating in favor or BindingConfiguration/BindingContext attach validatation. Then make internal.
        // Or at least rename to "ConfigureBinding" or similar
        public void BindConstructorArgumentFromValue(ParameterInfo parameter, IValueDescriptor valueDescriptor)
        {
            var constructor = FindConstructorOrThrow(parameter, "Parameter must be declared on a constructor.");
            var ctorDesc = FindModelConstructorDescriptor(constructor);

            if (ctorDesc is null)
            {
                throw new ArgumentException(paramName: nameof(parameter),
                    message: "Parameter is not described by any of the model constructor descriptors.");
            }

            var paramDesc = ctorDesc.ParameterDescriptors[parameter.Position];
            ConstructorArgumentBindingSources[paramDesc] = new SpecificSymbolValueSource(valueDescriptor);
        }

        public void BindMemberFromValue(PropertyInfo property, IValueDescriptor valueDescriptor)
        {
            var propertyDescriptor = FindModelPropertyDescriptor(property.PropertyType, property.Name);

            if (propertyDescriptor is null)
            {
                throw new ArgumentException(paramName: nameof(property),
                    message: "Property is not described by any of the model property descriptors.");
            }

            MemberBindingSources[propertyDescriptor] = new SpecificSymbolValueSource(valueDescriptor);
        }

        public object? CreateInstance(BindingContext bindingContext)
        {
            var (_, newInstance, _) = CreateInstanceInternal(bindingContext);
            return newInstance;
        }

        private (bool success, object? newInstance, bool anyNonDefaults) CreateInstanceInternal(
                    BindingContext bindingContext)
        {
            if (DisallowedBindingType())
            {
                throw new InvalidOperationException($"The type {ModelDescriptor.ModelType} cannot be bound");
            }
            if (ShortCutTheBinding(bindingContext))
            {
                return GetSimpleModelValue(MemberBindingSources, bindingContext);
            }
            var constructorAndArgs = GetBestConstructorAndArgs(bindingContext);
            var constructor = constructorAndArgs.Constructor;
            var boundValues = constructorAndArgs.BoundValues;
            bool nonDefaultsUsed = constructorAndArgs.NonDefaultsUsed;
            return constructor is null
                ? GetSimpleModelValue(ConstructorArgumentBindingSources, bindingContext)
                : InstanceFromSpecificConstructor(bindingContext, constructor, boundValues, ref nonDefaultsUsed);
        }

        private bool DisallowedBindingType()
        {
            var modelType = ModelDescriptor.ModelType;
            return modelType.IsConstructedGenericTypeOf(typeof(Span<>)) || 
                modelType.IsConstructedGenericTypeOf(typeof(ReadOnlySpan<>));
        }

        private bool ShortCutTheBinding(BindingContext bindingContext)
        {
            var modelType = ModelDescriptor.ModelType;
            return modelType.IsPrimitive ||
                   modelType.IsNullableValueType() ||
                   modelType == typeof(string);
        }

        private (bool success, object? newInstance, bool anyNonDefaults) GetSimpleModelValue(
                IDictionary<IValueDescriptor, IValueSource>? bindingSources, BindingContext bindingContext)
        {
            var valueSource = GetValueSource(bindingSources, bindingContext, ValueDescriptor, EnforceExplicitBinding);
            return bindingContext.TryBindToScalarValue(ValueDescriptor,
                                                       valueSource,
                                                       out var boundValue)
                ? (true, boundValue?.Value, true)
                : (false,(object?) null, false);
        }

        private (bool success, object? newInstance, bool anyNonDefaults) InstanceFromSpecificConstructor(
                BindingContext bindingContext, ConstructorDescriptor constructor, IReadOnlyList<BoundValue>? boundValues, ref bool nonDefaultsUsed)
        {
            var values = boundValues.Select(x => x.Value).ToArray();
            object? newInstance = null;
            try
            {
                newInstance = constructor.Invoke(values);
            }
            catch
            {
                return (false, null, false);
            }
            if (!(newInstance is null))
            {
                nonDefaultsUsed = UpdateInstanceInternalNotifyIfNonDefaultsUsed(newInstance, bindingContext);
            }
            return (true, newInstance, nonDefaultsUsed);
        }

        public void UpdateInstance<T>(T instance, BindingContext bindingContext)
            => UpdateInstanceInternalNotifyIfNonDefaultsUsed(instance, bindingContext);

        private bool UpdateInstanceInternalNotifyIfNonDefaultsUsed<T>(T instance, BindingContext bindingContext)
        {
            var (boundValues, anyNonDefaults) = GetBoundValues(
                MemberBindingSources,
                bindingContext,
                ModelDescriptor.PropertyDescriptors,
                EnforceExplicitBinding,
                ModelDescriptor.ModelType,
                includeMissingValues: false);

            foreach (var boundValue in boundValues)
            {
                ((PropertyDescriptor)boundValue.ValueDescriptor).SetValue(instance, boundValue.Value);
            }

            return anyNonDefaults;
        }

        private ConstructorAndArgs GetBestConstructorAndArgs(BindingContext bindingContext)
        {
            var constructorDescriptors =
                  ModelDescriptor
                      .ConstructorDescriptors
                      .OrderByDescending(d => d.ParameterDescriptors.Count);
            ConstructorAndArgs? bestNonMatching = null;
            foreach (var constructor in constructorDescriptors)
            {
                var (boundValues, anyNonDefaults) = GetBoundValues(
                        ConstructorArgumentBindingSources,
                        bindingContext,
                        constructor.ParameterDescriptors,
                        EnforceExplicitBinding,
                        ModelDescriptor.ModelType,
                        true);

                if (boundValues.Count == constructor.ParameterDescriptors.Count)
                {
                    var match = new ConstructorAndArgs(constructor, boundValues, anyNonDefaults);
                    if (anyNonDefaults)
                    { // based on parameter length, first usable constructor that utilizes CLI definition
                        return match;
                    }
                    bestNonMatching ??= match;
                }
            }
            return bestNonMatching is null
                    ? new ConstructorAndArgs(null, null, false)
                    : bestNonMatching;
        }

        internal static (IReadOnlyList<BoundValue> boundValues, bool anyNonDefaults) GetBoundValues(
                IDictionary<IValueDescriptor, IValueSource>? bindingSources,
                BindingContext bindingContext,
                IReadOnlyList<IValueDescriptor> valueDescriptors,
                bool enforceExplicitBinding,
                Type? parentType = null,
                bool includeMissingValues = true)
        {
            var values = new List<BoundValue>(valueDescriptors.Count);
            var anyNonDefaults = false;

            for (var index = 0; index < valueDescriptors.Count; index++)
            {
                var valueDescriptor = valueDescriptors[index];
                var valueSource = GetValueSource(bindingSources, bindingContext, valueDescriptor, enforceExplicitBinding);
                var (boundValue, usedNonDefault) = 
                    GetBoundValue(valueSource, bindingContext, valueDescriptor, includeMissingValues, parentType);
                if (usedNonDefault && !anyNonDefaults)
                {
                    anyNonDefaults = true;
                }
                if (boundValue != null)
                {
                    values.Add(boundValue);
                }
            }

            return (values, anyNonDefaults);
        }

        internal static IValueSource GetValueSource(IDictionary<IValueDescriptor, IValueSource>? bindingSources,
                                                    BindingContext bindingContext,
                                                    IValueDescriptor valueDescriptor,
                                                    bool enforceExplicitBinding)
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

            if (!enforceExplicitBinding)
            {
                // Return a value source that will match from the parseResult
                // by name and type (or a possible conversion)
                return new ParseResultMatchingValueSource();
            }

            return new MissingValueSource();
        }

        internal static (BoundValue? boundValue, bool usedNonDefault) GetBoundValue(
                    IValueSource valueSource,
                    BindingContext bindingContext,
                    IValueDescriptor valueDescriptor,
                    bool includeMissingValues,
                    Type? parentType)
        {
            if (bindingContext.TryBindToScalarValue(
                    valueDescriptor,
                    valueSource,
                    out var boundValue))
            {
                return (boundValue, true);
            }

            if (valueDescriptor.HasDefaultValue)
            {
                return (BoundValue.DefaultForValueDescriptor(valueDescriptor), false);
            }

            if (valueDescriptor.ValueType != parentType) // Recursive models aren't allowed
            {
                var binder = bindingContext.GetModelBinder(valueDescriptor);
                var (success, newInstance, usedNonDefaults) = binder.CreateInstanceInternal(bindingContext);
                if (success)
                {
                    return (new BoundValue(newInstance, valueDescriptor, valueSource), usedNonDefaults);
                }
            }

            if (includeMissingValues)
            {
                if (valueDescriptor is ParameterDescriptor parameterDescriptor && parameterDescriptor.AllowsNull)
                {
                    return (new BoundValue(parameterDescriptor.GetDefaultValue(), valueDescriptor, valueSource), false);
                }
                // Logic dropped here - misnamed and purpose unclear: ShouldPassNullToConstructor(constructorDescriptor.Parent, constructorDescriptor))
                return (BoundValue.DefaultForType(valueDescriptor), false);
            }
            return (null, false);
        }


        protected ConstructorDescriptor FindModelConstructorDescriptor(ConstructorInfo constructorInfo)
        {
            var constructorParameters = constructorInfo.GetParameters();

            return ModelDescriptor.ConstructorDescriptors
                .FirstOrDefault(ctorDesc
                        => ModelDescriptor.ModelType == constructorInfo.DeclaringType &&
                           ctorDesc.ParameterDescriptors
                                   .Any(x => constructorParameters.Any(y => MatchParameter(x, y))));

            static bool MatchParameter(ParameterDescriptor desc, ParameterInfo info)
            {
                return desc.ValueType == info.ParameterType &&
                       desc.ValueName == info.Name &&
                       desc.HasDefaultValue == info.HasDefaultValue &&
                       desc.AllowsNull == ParameterDescriptor.CalculateAllowsNull(info);
            }
        }

        protected IValueDescriptor FindModelPropertyDescriptor(Type propertyType, string propertyName)
        {
            return ModelDescriptor.PropertyDescriptors
                .FirstOrDefault(desc =>
                    desc.ValueType == propertyType &&
                    string.Equals(desc.ValueName, propertyName, StringComparison.Ordinal)
                    );
        }

        private ConstructorInfo FindConstructorOrThrow(ParameterInfo parameter, string message)
        {
            if (!(parameter.Member is ConstructorInfo constructor))
            {
                throw new ArgumentException(paramName: nameof(parameter),
                      message: message);
            }
            return constructor;
        }

        internal class AnonymousValueDescriptor : IValueDescriptor
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

    internal class ConstructorAndArgs
    {
        public ConstructorDescriptor? Constructor { get; }
        public IReadOnlyList<BoundValue>? BoundValues { get; }
        public bool NonDefaultsUsed { get; }

        public ConstructorAndArgs(ConstructorDescriptor? constructor, IReadOnlyList<BoundValue>? boundValues, bool nonDefaultsUsed)
        {
            Constructor = constructor;
            BoundValues = boundValues;
            NonDefaultsUsed = nonDefaultsUsed;
        }
    }
}
