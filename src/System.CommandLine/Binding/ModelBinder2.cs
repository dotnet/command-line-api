using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

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
                throw new ArgumentException(paramName: nameof(parameter),
                    message: "Parameter is not described by any of the model constructor descriptors.");

            var paramDesc = ctorDesc.ParameterDescriptors[parameter.Position];
            ConstructorArgumentBindingSources[paramDesc] = new SpecificSymbolValueSource(valueDescriptor);
        }

        public void BindMemberFromValue(PropertyInfo property, IValueDescriptor valueDescriptor)
        {
            var propertyDescriptor = FindModelPropertyDescriptor(property.PropertyType, property.Name);

            if (propertyDescriptor is null)
                throw new ArgumentException(paramName: nameof(property),
                    message: "Property is not described by any of the model property descriptors.");

            MemberBindingSources[propertyDescriptor] = new SpecificSymbolValueSource(valueDescriptor);
        }

        public object? CreateInstance(BindingContext bindingContext)
        {
            var (_, newInstance, _) = CreateInstanceInternal(bindingContext, true);
            return newInstance;
        }

        private (bool success, object? newInstance, bool anyNonDefaults) CreateInstanceInternal(
                    BindingContext bindingContext,
                    bool throwIfNoConstructor)
        {
            var constructorAndArgs = GetConstructorAndAgs(bindingContext);
            var constructor = constructorAndArgs.Constructor;
            var boundValues = constructorAndArgs.BoundValues;
            if (constructor is null)
            {
                return throwIfNoConstructor 
                    ? throw new InvalidOperationException("No appropriate constructor found") 
                    : ((bool success, object? newInstance, bool anyNonDefaults))(false, null, false);
            }

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
                UpdateInstance(newInstance, bindingContext);
            }
            return (true, newInstance, constructorAndArgs.NonDefaultsUsed);
        }

        public void UpdateInstance<T>(T instance, BindingContext bindingContext)
        {
            var (boundValues, anyNonDefaults) = GetValues(
                MemberBindingSources,
                bindingContext,
                ModelDescriptor.PropertyDescriptors,
                includeMissingValues: false);

            foreach (var boundValue in boundValues)
            {
                ((PropertyDescriptor)boundValue.ValueDescriptor).SetValue(instance, boundValue.Value);
            }
        }

        private ConstructorAndArgs GetConstructorAndAgs(BindingContext bindingContext)
        {
            var constructorDescriptors =
                  ModelDescriptor
                      .ConstructorDescriptors
                      .OrderByDescending(d => d.ParameterDescriptors.Count);
            foreach (var constructor in constructorDescriptors)
            {
                var (boundValues, anyNonDefaults) = GetValues(
                    ConstructorArgumentBindingSources,
                    bindingContext,
                    constructor.ParameterDescriptors,
                    true);

                if (boundValues.Count == constructor.ParameterDescriptors.Count)
                {
                    return new ConstructorAndArgs (constructor, boundValues, anyNonDefaults);
                }
            }
            return new ConstructorAndArgs(null, null, false);
        }


        private (IReadOnlyList<BoundValue> boundValues, bool anyNonDefaults) GetValues(
                IDictionary<IValueDescriptor, IValueSource>? bindingSources,
                BindingContext bindingContext,
                IReadOnlyList<IValueDescriptor> valueDescriptors,
                bool includeMissingValues)
        {
            var values = new List<BoundValue>();
            var anyNonDefaults = false;

            for (var index = 0; index < valueDescriptors.Count; index++)
            {
                var valueDescriptor = valueDescriptors[index];
                var valueSource = GetValueSource(bindingSources, bindingContext, valueDescriptor);
                var (boundValue, usedNonDefault) = GetBoundValue(valueSource, bindingContext, valueDescriptor, includeMissingValues, ModelDescriptor);
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

        internal static (BoundValue? boundValue, bool usedNonDefault) GetBoundValue(
                    IValueSource valueSource,
                    BindingContext bindingContext,
                    IValueDescriptor valueDescriptor,
                    bool includeMissingValues,
                    ModelDescriptor? modelDescriptor = null)
        {
            BoundValue? boundValue;
            if (bindingContext.TryBindToScalarValue(
                    valueDescriptor,
                    valueSource,
                    out boundValue))
            {
                return (boundValue, true);
            }

            if (valueDescriptor.HasDefaultValue)
            {
                return (BoundValue.DefaultForValueDescriptor(valueDescriptor), false);
            }

            if (!(modelDescriptor is null) && valueDescriptor.ValueType == modelDescriptor.ModelType)
            {
                throw new NotImplementedException("Recursive models are not allowed.");
            }
            var binder = bindingContext.GetModelBinder(valueDescriptor);
            // if there are constructors with parameters, we will try to bind
            var (success, newInstance, usedNonDefaults) = binder.CreateInstanceInternal(bindingContext, false);
            if (success && usedNonDefaults)
            {
                return (new BoundValue(newInstance, valueDescriptor, valueSource), true);
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

        private class ConstructorDescriptorEquality : IEqualityComparer<ParameterDescriptor>
        {
            public bool Equals(ParameterDescriptor x, ParameterDescriptor y)
            {
                return x.ValueType == x.ValueType &&
                       x.AllowsNull == x.AllowsNull &&
                       x.HasDefaultValue == x.HasDefaultValue;
            }

            public int GetHashCode(ParameterDescriptor obj)
            {
                throw new NotImplementedException();
            }
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

    internal struct ConstructorAndArgs
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
