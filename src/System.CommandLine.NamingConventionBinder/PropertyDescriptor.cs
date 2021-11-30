// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.Reflection;

namespace System.CommandLine.NamingConventionBinder;

/// <inheritdoc />
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

    /// <inheritdoc />
    public string ValueName => _propertyInfo.Name;

    /// <summary>
    /// The model descriptor that the target property belongs to.
    /// </summary>
    public ModelDescriptor Parent { get; }

    internal string Path => Parent + "." + ValueName;

    /// <inheritdoc />
    public Type ValueType => _propertyInfo.PropertyType;

    /// <inheritdoc />
    public bool HasDefaultValue => false;

    /// <inheritdoc />
    public object? GetDefaultValue() => ArgumentConverter.GetDefaultValue(ValueType);

    /// <summary>
    /// Sets a value on the target property.
    /// </summary>
    /// <param name="instance">The instance on which to set a value.</param>
    /// <param name="value">The value to set.</param>
    public void SetValue(object? instance, object? value)
    {
        _propertyInfo.SetValue(instance, value);
    }

    /// <inheritdoc />
    public override string ToString() => $"{ValueType.Name} {Path}";
}