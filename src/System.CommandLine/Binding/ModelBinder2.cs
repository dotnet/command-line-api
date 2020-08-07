using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

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

        private (bool success, object? newInstance, bool usedParameterlessConstructor) CreateInstanceInternal(BindingContext bindingContext,
                                                                           bool throwIfNoConstructor)
        {
            var (constructor, boundValues) = GetConstructorAndAgs(bindingContext);
            if (constructor is null)
            {
                if (throwIfNoConstructor)
                {
                    throw new InvalidOperationException("No appropriate constructor found");
                }
                return (false, null, false);
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
            return (true, newInstance, constructor.ParameterDescriptors.Any());
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

        private (ConstructorDescriptor?, IReadOnlyList<BoundValue>?) GetConstructorAndAgs(BindingContext bindingContext)
        {
            var constructorDescriptors =
                  ModelDescriptor
                      .ConstructorDescriptors
                      .OrderByDescending(d => d.ParameterDescriptors.Count);
            foreach (var constructor in constructorDescriptors)
            {
                var boundConstructorArguments = GetValues(
                    ConstructorArgumentBindingSources,
                    bindingContext,
                    constructor.ParameterDescriptors,
                    true);

                if (boundConstructorArguments.Count == constructor.ParameterDescriptors.Count)
                {
                    return (constructor, boundConstructorArguments);
                }
            }
            return (null, null);
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
                BoundValue? boundValue = GetBoundValue(valueSource, bindingContext, valueDescriptor, includeMissingValues, ModelDescriptor);

                //if (boundValue is null)
                //{

                //    var binder = bindingContext.GetModelBinder(valueDescriptor);
                //    // if there are constructors with parameters, we will try to bind

                //    if (boundValue is null && includeMissingValues)
                //    {
                //        if (valueDescriptor.HasDefaultValue)
                //        {
                //            boundValue = BoundValue.DefaultForValueDescriptor(valueDescriptor);
                //        }
                //        if (valueDescriptor.ValueType == ModelDescriptor.ModelType)
                //        {
                //            throw new NotImplementedException("Recursive models are not allowed.");
                //        }
                //        var (success, newInstance) = binder.CreateInstanceInternal(bindingContext, false);
                //        if (success)
                //        {
                //            // might change to early loop, but this might make flow more clear
                //            boundValue = new BoundValue(newInstance, valueDescriptor, valueSource);
                //        }

                //        if (valueDescriptor is ParameterDescriptor parameterDescriptor)
                //        {
                //            else if (parameterDescriptor.AllowsNull)
                //                // Logic dropped here - misnamed and purpose unclear: ShouldPassNullToConstructor(constructorDescriptor.Parent, constructorDescriptor))
                //                boundValue = BoundValue.DefaultForType(valueDescriptor);
                //        }
                //    }
                //}

                if (boundValue != null)
                {
                    values.Add(boundValue);
                }
            }

            return values;
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

        internal static BoundValue? GetBoundValue(IValueSource valueSource,
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
                return boundValue;
            }

            if (valueDescriptor.HasDefaultValue)
            {
                return BoundValue.DefaultForValueDescriptor(valueDescriptor);
            }

            if (!(modelDescriptor is null) && valueDescriptor.ValueType == modelDescriptor.ModelType)
            {
                throw new NotImplementedException("Recursive models are not allowed.");
            }

            var binder = bindingContext.GetModelBinder(valueDescriptor);
            // if there are constructors with parameters, we will try to bind
            var (success, newInstance, usedParameterlessConstructor) = binder.CreateInstanceInternal(bindingContext, false);
            if (success && usedParameterlessConstructor)
            {
                return new BoundValue(newInstance, valueDescriptor, valueSource);
            }

            if (includeMissingValues)
            {
                if (valueDescriptor is ParameterDescriptor parameterDescriptor && parameterDescriptor.AllowsNull)
                {
                    return new BoundValue(parameterDescriptor.GetDefaultValue(), valueDescriptor, valueSource);
                }
                // Logic dropped here - misnamed and purpose unclear: ShouldPassNullToConstructor(constructorDescriptor.Parent, constructorDescriptor))
                return BoundValue.DefaultForType(valueDescriptor);
            }
            return null;
        }
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
}
