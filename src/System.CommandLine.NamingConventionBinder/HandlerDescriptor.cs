// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using System.Reflection;

namespace System.CommandLine.NamingConventionBinder;

/// <inheritdoc />
public abstract class HandlerDescriptor : IMethodDescriptor
{
    private List<ParameterDescriptor>? _parameterDescriptors;

    /// <summary>
    /// Gets a command handler that can bind to the specifications of the descriptor.
    /// </summary>
    public abstract ICommandHandler GetCommandHandler();

    /// <inheritdoc />
    public abstract ModelDescriptor? Parent { get; }

    /// <inheritdoc />
    public IReadOnlyList<ParameterDescriptor> ParameterDescriptors =>
        _parameterDescriptors ??= new List<ParameterDescriptor>(InitializeParameterDescriptors());

    private protected abstract IEnumerable<ParameterDescriptor> InitializeParameterDescriptors();

    /// <inheritdoc />
    public override string ToString() =>
        $"{Parent} ({string.Join(", ", ParameterDescriptors)})";

    /// <summary>
    /// Creates a descriptor based on the specified method.
    /// </summary>
    /// <param name="methodInfo">The method for which to create a handler descriptor.</param>
    /// <param name="target">An instance for the descriptor to target. In the case of <see langword="static"/> methods, <see langword="null"/> should be passed.</param>
    /// <returns>A handler descriptor for the method and instance.</returns>
    public static HandlerDescriptor FromMethodInfo(MethodInfo methodInfo, object? target = null) =>
        new MethodInfoHandlerDescriptor(methodInfo, target);

    /// <summary>
    /// Creates a descriptor based on the specified method.
    /// </summary>
    /// <param name="delegate">The delegate for which to create a handler descriptor.</param>
    /// <returns>A handler descriptor for the delegate.</returns>
    public static HandlerDescriptor FromDelegate(Delegate @delegate) =>
        new DelegateHandlerDescriptor(@delegate);
}