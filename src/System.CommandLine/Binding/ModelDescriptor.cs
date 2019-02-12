// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine.Binding
{
    public class ModelDescriptor
    {
        private static readonly Dictionary<Type, ModelDescriptor> _modelDescriptors = new Dictionary<Type, ModelDescriptor>();

        private readonly List<PropertyDescriptor> _propertyDescriptors = new List<PropertyDescriptor>();
        private readonly List<ConstructorDescriptor> _constructorDescriptors = new List<ConstructorDescriptor>();

        protected ModelDescriptor(Type modelType)
        {
            ModelType = modelType ??
                        throw new ArgumentNullException(nameof(modelType));

            foreach (var propertyInfo in modelType.GetProperties(ReflectionBinder.CommonBindingFlags).Where(p => p.CanWrite))
            {
                _propertyDescriptors.Add(new PropertyDescriptor(propertyInfo));
            }

            foreach (var constructorInfo in modelType.GetConstructors(ReflectionBinder.CommonBindingFlags))
            {
                _constructorDescriptors.Add(new ConstructorDescriptor(constructorInfo));
            }
        }

        public IReadOnlyList<ConstructorDescriptor> ConstructorDescriptors => _constructorDescriptors;

        public IReadOnlyList<IValueDescriptor> PropertyDescriptors => _propertyDescriptors;

        public Type ModelType { get; }

        public static ModelDescriptor<T> FromType<T>()
        {
            return (ModelDescriptor<T>)_modelDescriptors.GetOrAdd(typeof(T), _ => new ModelDescriptor<T>());
        }

        public static ModelDescriptor FromType(Type type)
        {
            return _modelDescriptors.GetOrAdd(
                type,
                _ => new ModelDescriptor(type));
        }
    }
}
