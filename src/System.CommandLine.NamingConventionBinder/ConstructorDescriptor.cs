// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.Linq;
using System.Reflection;

namespace System.CommandLine.NamingConventionBinder;

/// <summary>
/// Provides information for binding command line input to a constructor.
/// </summary>
public class ConstructorDescriptor : IMethodDescriptor
{
    private List<ParameterDescriptor>? _parameterDescriptors;

    private readonly ConstructorInfo _constructorInfo;

    internal ConstructorDescriptor(
        ConstructorInfo constructorInfo,
        ModelDescriptor parent)
    {
        Parent = parent;
        _constructorInfo = constructorInfo;
    }

    /// <inheritdoc />
    public ModelDescriptor Parent { get; }

    /// <inheritdoc />
    public IReadOnlyList<ParameterDescriptor> ParameterDescriptors =>
        _parameterDescriptors ??=
            _constructorInfo.GetParameters().Select(p => new ParameterDescriptor(p, this)).ToList();

    internal object Invoke(IReadOnlyCollection<object?> parameters)
    {
        return _constructorInfo.Invoke(parameters.ToArray());
    }

    /// <inheritdoc />
    public override string ToString() =>
        $"{Parent} ({string.Join(", ", ParameterDescriptors)})";
}