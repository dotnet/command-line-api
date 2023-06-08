// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace System.CommandLine.NamingConventionBinder;

/// <summary>
/// Instantiates values to be passed to a user-defined command handler method.
/// </summary>
public class ModelBindingCommandHandler : BindingHandler
{
    private readonly Delegate? _handlerDelegate;
    private readonly object? _invocationTarget;
    private readonly ModelBinder? _invocationTargetBinder;
    private readonly MethodInfo? _handlerMethodInfo;
    private readonly IMethodDescriptor _methodDescriptor;
    private Dictionary<IValueDescriptor, IValueSource> _invokeArgumentBindingSources { get; } =
        new();

    internal ModelBindingCommandHandler(
        MethodInfo handlerMethodInfo,
        IMethodDescriptor methodDescriptor,
        object? invocationTarget)
    {
        _handlerMethodInfo = handlerMethodInfo ?? throw new ArgumentNullException(nameof(handlerMethodInfo));
        _invocationTargetBinder = _handlerMethodInfo.IsStatic
                                      ? null
                                      : new ModelBinder(_handlerMethodInfo.ReflectedType!);
        _methodDescriptor = methodDescriptor ?? throw new ArgumentNullException(nameof(methodDescriptor));
        _invocationTarget = invocationTarget;
    }

    internal ModelBindingCommandHandler(
        MethodInfo handlerMethodInfo,
        IMethodDescriptor methodDescriptor)
        : this(handlerMethodInfo, methodDescriptor, null)
    { }

    internal ModelBindingCommandHandler(
        Delegate handlerDelegate,
        IMethodDescriptor methodDescriptor)
    {
        _handlerDelegate = handlerDelegate ?? throw new ArgumentNullException(nameof(handlerDelegate));
        _methodDescriptor = methodDescriptor ?? throw new ArgumentNullException(nameof(methodDescriptor));
    }

    /// <summary>
    /// Binds values for the underlying user-defined method and uses them to invoke that method.
    /// </summary>
    /// <param name="parseResult">The current parse result.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the invocation.</param>
    /// <returns>A task whose value can be used to set the process exit code.</returns>
    public override async Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
    {
        var bindingContext = GetBindingContext(parseResult);

        var (boundValues, _) = ModelBinder.GetBoundValues(
            _invokeArgumentBindingSources,
            bindingContext,
            _methodDescriptor.ParameterDescriptors,
            false);

        var invocationArguments = boundValues
                                  .Select(x => x.Value)
                                  .ToArray();

        object? result;
        if (_handlerDelegate is null)
        {
            var invocationTarget = _invocationTarget ?? 
                                   bindingContext.GetService(_handlerMethodInfo!.ReflectedType!);
            if(invocationTarget is { })
            {
                _invocationTargetBinder?.UpdateInstance(invocationTarget, bindingContext);
            }

            invocationTarget ??= _invocationTargetBinder?.CreateInstance(bindingContext);
            result = _handlerMethodInfo!.Invoke(invocationTarget, invocationArguments);
        }
        else
        {
            result = _handlerDelegate.DynamicInvoke(invocationArguments);
        }

        return await CommandHandler.GetExitCodeAsync(result);
    }

    /// <summary>
    /// Binds a method or constructor parameter based on the specified <see cref="CliArgument"/>.
    /// </summary>
    /// <param name="param">The parameter to bind.</param>
    /// <param name="argument">The argument whose parsed result will be the source of the bound value.</param>
    public void BindParameter(ParameterInfo param, CliArgument argument)
    {
        var _ = argument ?? throw new InvalidOperationException("You must specify an argument to bind");
        BindValueSource(param, new SpecificSymbolValueSource(argument));
    }

    /// <summary>
    /// Binds a method or constructor parameter based on the specified <see cref="CliOption"/>.
    /// </summary>
    /// <param name="param">The parameter to bind.</param>
    /// <param name="option">The option whose parsed result will be the source of the bound value.</param>
    public void BindParameter(ParameterInfo param, CliOption option)
    {
        var _ = option ?? throw new InvalidOperationException("You must specify an option to bind");
        BindValueSource(param, new SpecificSymbolValueSource(option));
    }

    private void BindValueSource(ParameterInfo param, IValueSource valueSource)
    {
        var paramDesc = FindParameterDescriptor(param);
        if (paramDesc is null)
        {
            throw new InvalidOperationException("You must bind to a parameter on this handler");
        }
        _invokeArgumentBindingSources.Add(paramDesc, valueSource);
    }

    private ParameterDescriptor? FindParameterDescriptor(ParameterInfo? param)
        => param is null
               ? null
               : _methodDescriptor.ParameterDescriptors
                                  .FirstOrDefault(x => x.ValueName == param.Name &&
                                                       x.ValueType == param.ParameterType);
}