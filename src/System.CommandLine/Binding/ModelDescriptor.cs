// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace System.CommandLine.Binding
{
    public class ModelDescriptor
    {
        private const BindingFlags CommonBindingFlags =
            BindingFlags.IgnoreCase
            | BindingFlags.Public
            | BindingFlags.Instance;

        private static readonly ConcurrentDictionary<Type, ModelDescriptor> _modelDescriptors = new();

        private List<PropertyDescriptor>? _propertyDescriptors;
        private List<ConstructorDescriptor>? _constructorDescriptors;

        protected ModelDescriptor(Type modelType)
        {
            ModelType = modelType ??
                        throw new ArgumentNullException(nameof(modelType));
        }

        public IReadOnlyList<ConstructorDescriptor> ConstructorDescriptors =>
            _constructorDescriptors ??=
                ModelType.GetConstructors(CommonBindingFlags)
                         .Select(i => new ConstructorDescriptor(i, this))
                         .ToList();

        public IReadOnlyList<IValueDescriptor> PropertyDescriptors =>
            _propertyDescriptors ??=
                ModelType.GetProperties(CommonBindingFlags)
                         .Where(p => p.CanWrite && p.SetMethod.IsPublic)
                         .Select(i => new PropertyDescriptor(i, this))
                         .ToList();

        public Type ModelType { get; }

        /// <inheritdoc />
        public override string ToString() => $"{ModelType.Name}";

        public static ModelDescriptor FromType<T>() =>
            _modelDescriptors.GetOrAdd(
                typeof(T),
                _ => new ModelDescriptor(typeof(T)));

        public static ModelDescriptor FromType(Type type) =>
            _modelDescriptors.GetOrAdd(
                type,
                _ => new ModelDescriptor(type));
    }
}