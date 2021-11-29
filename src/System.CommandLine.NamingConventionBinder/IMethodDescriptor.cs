// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;

namespace System.CommandLine.NamingConventionBinder;

/// <summary>
/// Provides information for binding command line input to a method.
/// </summary>
public interface IMethodDescriptor
{
    /// <summary>
    /// The model descriptor that this constructor belongs to.
    /// </summary>
    ModelDescriptor? Parent { get; }

    /// <summary>
    /// Descriptors for the parameters of the constructor.
    /// </summary>
    IReadOnlyList<ParameterDescriptor> ParameterDescriptors { get; }
}