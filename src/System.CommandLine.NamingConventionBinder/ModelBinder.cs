// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.Linq;
using System.Reflection;

namespace System.CommandLine.NamingConventionBinder;

/// <summary>
/// Creates instances of a specified type by binding properties and constructor parameters from command line input.
/// </summary>
public class ModelBinder
{
    /// <param name="modelType">The type that the model binder can bind.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public ModelBinder(Type modelType)
        : this(new AnonymousValueDescriptor(modelType))
        => _ = modelType ?? throw new ArgumentNullException(nameof(modelType));

    internal ModelBinder(IValueDescriptor valueDescriptor)
    {
        ValueDescriptor = valueDescriptor ?? throw new ArgumentNullException(nameof(valueDescriptor));
        ModelDescriptor = ModelDescriptor.FromType(valueDescriptor.ValueType);
    }

    /// <summary>
    /// A descriptor for the source value
    /// </summary>
    public IValueDescriptor ValueDescriptor { get; }
        
    /// <summary>
    /// The descriptor for the model type that the model binder targets.
    /// </summary>
    public ModelDescriptor ModelDescriptor { get; }

    /// <summary>
    /// When set to <see langword="true"/>, the model binder will only bind constructor parameters or properties that it has been explicitly configured to bind.
    /// </summary>
    public bool EnforceExplicitBinding { get; set; }
    
    internal Dictionary<IValueDescriptor, IValueSource> ConstructorArgumentBindingSources { get; } =
        new();

    internal Dictionary<IValueDescriptor, IValueSource> MemberBindingSources { get; } =
        new();

    /// <summary>
    /// Sets a property using a value descriptor.
    /// </summary>
    /// <param name="property">The property to bind.</param>
    /// <param name="valueDescriptor">A descriptor of the value to be used to set the property.</param>
    /// <exception cref="ArgumentException"></exception>
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

    /// <summary>
    /// Creates an instance of the target model type.
    /// </summary>
    /// <param name="bindingContext">The binding context from which values are resolved.</param>
    /// <returns>An instance created from the values in the binding context.</returns>
    public object? CreateInstance(BindingContext bindingContext)
    {
        var (_, newInstance, _) = CreateInstanceInternal(bindingContext);
        return newInstance;
    }

    /// <summary>
    /// Updates an instance of the target model type.
    /// </summary>
    public void UpdateInstance<T>(T instance, BindingContext bindingContext)
        => UpdateInstanceInternalNotifyIfNonDefaultsUsed(instance, bindingContext);

    private (bool success, object? newInstance, bool anyNonDefaults) CreateInstanceInternal(BindingContext bindingContext)
    {
        if (IsModelTypeUnbindable())
        {
            throw new InvalidOperationException($"The type {ModelDescriptor.ModelType} cannot be bound");
        }

        if (ShortCutTheBinding())
        {
            return GetSimpleModelValue(MemberBindingSources, bindingContext);
        }

        var constructorAndArgs = GetBestConstructorAndArgs(bindingContext);

        if (constructorAndArgs is null)
        {
            return GetSimpleModelValue(ConstructorArgumentBindingSources, bindingContext);
        }
        else
        {
            var constructor = constructorAndArgs.Constructor;
            var boundValues = constructorAndArgs.BoundValues;
            var nonDefaultsUsed = constructorAndArgs.NonDefaultsUsed;

            return InstanceFromSpecificConstructor(bindingContext, constructor, boundValues, ref nonDefaultsUsed);
        }
    }

    private bool IsModelTypeUnbindable()
    {
        var modelType = ModelDescriptor.ModelType;
        return modelType.IsConstructedGenericTypeOf(typeof(Span<>)) ||
               modelType.IsConstructedGenericTypeOf(typeof(ReadOnlySpan<>));
    }

    private bool ShortCutTheBinding()
    {
        var modelType = ModelDescriptor.ModelType;
        return modelType.IsPrimitive ||
               modelType.IsNullableValueType() ||
               modelType == typeof(string) ||
               modelType == typeof(decimal) ;
    }

    private (bool success, object? newInstance, bool anyNonDefaults) GetSimpleModelValue(
        IDictionary<IValueDescriptor, IValueSource> bindingSources, BindingContext bindingContext)
    {
        var valueSource = GetValueSource(bindingSources, bindingContext, ValueDescriptor, EnforceExplicitBinding);
        return bindingContext.TryBindToScalarValue(ValueDescriptor,
                                                   valueSource,
                                                   bindingContext.ParseResult.CommandResult.LocalizationResources,
                                                   out var boundValue)
                   ? (true, boundValue?.Value, true)
                   : (false, null, false);
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

        nonDefaultsUsed = UpdateInstanceInternalNotifyIfNonDefaultsUsed(newInstance, bindingContext);

        return (true, newInstance, nonDefaultsUsed);
    }

    private bool UpdateInstanceInternalNotifyIfNonDefaultsUsed<T>(T instance, BindingContext bindingContext)
    {
        var (boundValues, anyNonDefaults) = GetBoundValues(
            MemberBindingSources,
            bindingContext,
            ModelDescriptor.PropertyDescriptors,
            EnforceExplicitBinding,
            ModelDescriptor.ModelType,
            includeMissingValues: false);

        for (var i = 0; i < boundValues.Count; i++)
        {
            var boundValue = boundValues[i];
            ((PropertyDescriptor) boundValue.ValueDescriptor).SetValue(instance, boundValue.Value);
        }

        return anyNonDefaults;
    }

    private ConstructorAndArgs? GetBestConstructorAndArgs(BindingContext bindingContext)
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
                { 
                    // based on parameter length, first usable constructor that utilizes CLI definition
                    return match;
                }
                bestNonMatching ??= match;
            }
        }

        return bestNonMatching;
    }

    internal static (IReadOnlyList<BoundValue> boundValues, bool anyNonDefaults) GetBoundValues(
        IDictionary<IValueDescriptor, IValueSource> bindingSources,
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

            if (boundValue.HasValue)
            {
                values.Add(boundValue.Value);
            }
        }

        return (values, anyNonDefaults);
    }

    internal static IValueSource GetValueSource(IDictionary<IValueDescriptor, IValueSource> bindingSources,
        BindingContext bindingContext,
        IValueDescriptor valueDescriptor,
        bool enforceExplicitBinding)
    {
        if (bindingSources.TryGetValue(valueDescriptor, out IValueSource? valueSource))
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
                bindingContext.ParseResult.CommandResult.LocalizationResources,
                out var boundValue))
        {
            return (boundValue, true);
        }

        if (includeMissingValues)
        {
            if (valueDescriptor.HasDefaultValue)
            {
                return (DefaultForValueDescriptor(valueDescriptor, bindingContext), false);
            }

            if (valueDescriptor.ValueType != parentType) // Recursive models aren't allowed
            {
                var binder = bindingContext.GetOrCreateModelBinder(valueDescriptor);

                if (binder.IsModelTypeUnbindable())
                {
                    return (null, false);
                }

                var (success, newInstance, usedNonDefaults) = binder.CreateInstanceInternal(bindingContext);

                if (success)
                {
                    return (new BoundValue(newInstance, valueDescriptor, valueSource), usedNonDefaults);
                }
            }

            if (valueDescriptor is ParameterDescriptor { AllowsNull: true } parameterDescriptor)
            {
                return (new BoundValue(parameterDescriptor.GetDefaultValue(), valueDescriptor, valueSource), false);
            }

            return (DefaultForValueDescriptor(valueDescriptor, bindingContext), false);
        }

        return (null, false);
    }

    private static BoundValue DefaultForValueDescriptor(
        IValueDescriptor valueDescriptor,
        BindingContext context)
    {
        var valueSource = ValueDescriptorDefaultValueSource.Instance;

        valueSource.TryGetValue(valueDescriptor, context, out var value);

        return new BoundValue(
            value,
            valueDescriptor,
            valueSource);
    }

    private protected IValueDescriptor FindModelPropertyDescriptor(Type propertyType, string propertyName)
    {
        return ModelDescriptor.PropertyDescriptors
                              .FirstOrDefault(desc =>
                                                  desc.ValueType == propertyType &&
                                                  string.Equals(desc.ValueName, propertyName, StringComparison.Ordinal));
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

    internal class ConstructorAndArgs
    {
        public ConstructorDescriptor Constructor { get; }
        public IReadOnlyList<BoundValue>? BoundValues { get; }
        public bool NonDefaultsUsed { get; }

        public ConstructorAndArgs(ConstructorDescriptor constructor, IReadOnlyList<BoundValue>? boundValues, bool nonDefaultsUsed)
        {
            Constructor = constructor;
            BoundValues = boundValues;
            NonDefaultsUsed = nonDefaultsUsed;
        }
    }
}