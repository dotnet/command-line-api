// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;

namespace System.CommandLine.Binding
{
    public class PropertyDescriptor : IValueDescriptor
    {
        private readonly PropertyInfo _propertyInfo;

        internal PropertyDescriptor(
            PropertyInfo propertyInfo,
            ModelDescriptor parent)
        {
            Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            _propertyInfo = propertyInfo;
        }

        public string ValueName => _propertyInfo.Name;

        public ModelDescriptor Parent { get; }

        internal string Path => Parent + "." + ValueName;

        public Type ValueType => _propertyInfo.PropertyType;

        public bool HasDefaultValue => false;

        public object? GetDefaultValue() => Binder.GetDefaultValue(ValueType);

        public void SetValue(object? instance, object? value)
        {
            _propertyInfo.SetValue(instance, value);
        }

        /// <inheritdoc />
        public override string ToString() => $"{ValueType.Name} {Path}";
    }
}
