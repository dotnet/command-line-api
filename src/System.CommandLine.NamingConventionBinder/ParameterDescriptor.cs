// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.Reflection;

namespace System.CommandLine.NamingConventionBinder;

/// <summary>
/// Provides information for binding command line input to a method or constructor parameter.
/// </summary>
public class ParameterDescriptor : IValueDescriptor
{
    private readonly ParameterInfo _parameterInfo;
    private bool? _allowsNull;

    internal ParameterDescriptor(
        ParameterInfo parameterInfo,
        IMethodDescriptor parent)
    {
        Parent = parent;
        _parameterInfo = parameterInfo;
    }

    /// <inheritdoc />
    public string ValueName => _parameterInfo.Name;

    /// <summary>
    /// The method descriptor that this constructor belongs to.
    /// </summary>
    public IMethodDescriptor Parent { get; }

    /// <inheritdoc />
    public Type ValueType => _parameterInfo.ParameterType;

    /// <inheritdoc />
    public bool HasDefaultValue => _parameterInfo.HasDefaultValue;

    /// <summary>
    /// Gets a value indicating whether <see langword="null"/> is allowed to be passed for the target parameter.
    /// </summary>
    public bool AllowsNull
    {
        get
        {
            if (_allowsNull is null)
            {
                _allowsNull = CalculateAllowsNull(_parameterInfo);
            }
            return _allowsNull ?? false;
        }
    }

    internal static bool CalculateAllowsNull(ParameterInfo parameterInfo) 
        => parameterInfo.ParameterType.IsNullable() ||
           parameterInfo.HasDefaultValue && parameterInfo.DefaultValue is null;

    /// <inheritdoc />
    public object? GetDefaultValue() =>
        _parameterInfo.DefaultValue is DBNull
            ? ArgumentConverter.GetDefaultValue(ValueType)
            : _parameterInfo.DefaultValue;

    /// <inheritdoc />
    public override string ToString() => $"{ValueType.Name} {ValueName}";
}