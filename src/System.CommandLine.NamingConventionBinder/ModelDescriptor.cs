// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.CommandLine.Binding;
using System.Linq;
using System.Reflection;

namespace System.CommandLine.NamingConventionBinder;

/// <summary>
/// Provides information for binding command line input to instances of a specific <see cref="ModelType"/>.
/// </summary>
public class ModelDescriptor
{
    private const BindingFlags CommonBindingFlags =
        BindingFlags.IgnoreCase
        | BindingFlags.Public
        | BindingFlags.Instance;

    private static readonly ConcurrentDictionary<Type, ModelDescriptor> _modelDescriptors = new();

    private List<PropertyDescriptor>? _propertyDescriptors;
    private List<ConstructorDescriptor>? _constructorDescriptors;

    /// <param name="modelType">The type of the model.</param>
    protected ModelDescriptor(Type modelType)
    {
        ModelType = modelType ??
                    throw new ArgumentNullException(nameof(modelType));
    }

    /// <summary>
    /// Descriptors for the constructors for <see cref="ModelType"/>
    /// </summary>
    public IReadOnlyList<ConstructorDescriptor> ConstructorDescriptors =>
        _constructorDescriptors ??=
            ModelType.GetConstructors(CommonBindingFlags)
                     .Select(i => new ConstructorDescriptor(i, this))
                     .ToList();

    /// <summary>
    /// Descriptors for the properties of <see cref="ModelType"/>.
    /// </summary>
    public IReadOnlyList<IValueDescriptor> PropertyDescriptors =>
        _propertyDescriptors ??=
            ModelType.GetProperties(CommonBindingFlags)
                     .Where(p => p.CanWrite && p.SetMethod.IsPublic)
                     .Select(i => new PropertyDescriptor(i, this))
                     .ToList();

    /// <summary>
    /// The type that the model binder can bind instances of.
    /// </summary>
    public Type ModelType { get; }

    /// <inheritdoc />
    public override string ToString() => $"{ModelType.Name}";

    /// <summary>
    /// Creates a model binder for the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type that the model binder can bind instances of.</typeparam>
    /// <returns>A <see cref="ModelBinder"/> for type <typeparamref name="T"/>.</returns>
    public static ModelDescriptor FromType<T>() =>
        _modelDescriptors.GetOrAdd(
            typeof(T),
            _ => new ModelDescriptor(typeof(T)));

    /// <summary>
    /// Creates a model binder for the specified type.
    /// </summary>
    /// <param name="type">The type that the model binder can bind instances of.</param>
    /// <returns>A <see cref="ModelBinder"/> for the specified type.</returns>
    public static ModelDescriptor FromType(Type type) =>
        _modelDescriptors.GetOrAdd(
            type,
            _ => new ModelDescriptor(type));
}