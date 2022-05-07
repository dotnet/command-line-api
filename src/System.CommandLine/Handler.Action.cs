// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.CommandLine.Invocation;

namespace System.CommandLine;

/// <summary>
/// Provides methods for creating and working with command handlers.
/// </summary>
public static partial class Handler
{
    /// <summary>
    /// Sets a command's handler based on an <see cref="Action{InvocationContext}"/>.
    /// </summary>
    public static void SetHandler(
        this Command command,
        Action<InvocationContext> handle) =>
        command.Handler = new AnonymousCommandHandler(handle);

    /// <summary>
    /// Sets a command's handler based on an <see cref="Action"/>.
    /// </summary>
    public static void SetHandler(
        this Command command,
        Action handle) =>
        command.Handler = new AnonymousCommandHandler(_ => handle());

    /// <summary>
    /// Sets a command's handler based on an <see cref="Action{T}"/>.
    /// </summary>
    public static void SetHandler<T>(
        this Command command,
        Action<T> handle,
        IValueDescriptor<T> symbol) =>
        command.Handler = new AnonymousCommandHandler(
            context =>
            {
                var value1 = GetValueForHandlerParameter(symbol, context);

                handle(value1!);
            });

    /// <summary>
    /// Sets a command's handler based on an <see cref="Action{T1,T2}"/>.
    /// </summary>
    public static void SetHandler<T1, T2>(
        this Command command,
        Action<T1, T2> handle,
        IValueDescriptor<T1> symbol1,
        IValueDescriptor<T2> symbol2) =>
        command.Handler = new AnonymousCommandHandler(
            context =>
            {
                var value1 = GetValueForHandlerParameter(symbol1, context);
                var value2 = GetValueForHandlerParameter(symbol2, context);

                handle(value1!, value2!);
            });

    /// <summary>
    /// Sets a command's handler based on an <see cref="Action{T1,T2,T3}"/>.
    /// </summary>
    public static void SetHandler<T1, T2, T3>(
        this Command command,
        Action<T1, T2, T3> handle,
        IValueDescriptor<T1> symbol1,
        IValueDescriptor<T2> symbol2,
        IValueDescriptor<T3> symbol3) =>
        command.Handler = new AnonymousCommandHandler(
            context =>
            {
                var value1 = GetValueForHandlerParameter(symbol1, context);
                var value2 = GetValueForHandlerParameter(symbol2, context);
                var value3 = GetValueForHandlerParameter(symbol3, context);

                handle(value1!, value2!, value3!);
            });

    /// <summary>
    /// Sets a command's handler based on an <see cref="Action{T1,T2,T3,T4}"/>.
    /// </summary>
    public static void SetHandler<T1, T2, T3, T4>(
        this Command command,
        Action<T1, T2, T3, T4> handle,
        IValueDescriptor<T1> symbol1,
        IValueDescriptor<T2> symbol2,
        IValueDescriptor<T3> symbol3,
        IValueDescriptor<T4> symbol4) =>
        command.Handler = new AnonymousCommandHandler(
            context =>
            {
                var value1 = GetValueForHandlerParameter(symbol1, context);
                var value2 = GetValueForHandlerParameter(symbol2, context);
                var value3 = GetValueForHandlerParameter(symbol3, context);
                var value4 = GetValueForHandlerParameter(symbol4, context);

                handle(value1!, value2!, value3!, value4!);
            });

    /// <summary>
    /// Sets a command's handler based on an <see cref="Action{T1,T2,T3,T4,T5}"/>.
    /// </summary>
    public static void SetHandler<T1, T2, T3, T4, T5>(
        this Command command,
        Action<T1, T2, T3, T4, T5> handle,
        IValueDescriptor<T1> symbol1,
        IValueDescriptor<T2> symbol2,
        IValueDescriptor<T3> symbol3,
        IValueDescriptor<T4> symbol4,
        IValueDescriptor<T5> symbol5) =>
        command.Handler = new AnonymousCommandHandler(
            context =>
            {
                var value1 = GetValueForHandlerParameter(symbol1, context);
                var value2 = GetValueForHandlerParameter(symbol2, context);
                var value3 = GetValueForHandlerParameter(symbol3, context);
                var value4 = GetValueForHandlerParameter(symbol4, context);
                var value5 = GetValueForHandlerParameter(symbol5, context);

                handle(value1!, value2!, value3!, value4!, value5!);
            });

    /// <summary>
    /// Sets a command's handler based on an <see cref="Action{T1,T2,T3,T4,T5,T6}"/>.
    /// </summary>
    public static void SetHandler<T1, T2, T3, T4, T5, T6>(
        this Command command,
        Action<T1, T2, T3, T4, T5, T6> handle,
        IValueDescriptor<T1> symbol1,
        IValueDescriptor<T2> symbol2,
        IValueDescriptor<T3> symbol3,
        IValueDescriptor<T4> symbol4,
        IValueDescriptor<T5> symbol5,
        IValueDescriptor<T6> symbol6) =>
        command.Handler = new AnonymousCommandHandler(
            context =>
            {
                var value1 = GetValueForHandlerParameter(symbol1, context);
                var value2 = GetValueForHandlerParameter(symbol2, context);
                var value3 = GetValueForHandlerParameter(symbol3, context);
                var value4 = GetValueForHandlerParameter(symbol4, context);
                var value5 = GetValueForHandlerParameter(symbol5, context);
                var value6 = GetValueForHandlerParameter(symbol6, context);

                handle(value1!, value2!, value3!, value4!, value5!, value6!);
            });

    /// <summary>
    /// Sets a command's handler based on an <see cref="Action{T1,T2,T3,T4,T5,T6,T7}"/>.
    /// </summary>
    public static void SetHandler<T1, T2, T3, T4, T5, T6, T7>(
        this Command command,
        Action<T1, T2, T3, T4, T5, T6, T7> handle,
        IValueDescriptor<T1> symbol1,
        IValueDescriptor<T2> symbol2,
        IValueDescriptor<T3> symbol3,
        IValueDescriptor<T4> symbol4,
        IValueDescriptor<T5> symbol5,
        IValueDescriptor<T6> symbol6,
        IValueDescriptor<T7> symbol7) =>
        command.Handler = new AnonymousCommandHandler(
            context =>
            {
                var value1 = GetValueForHandlerParameter(symbol1, context);
                var value2 = GetValueForHandlerParameter(symbol2, context);
                var value3 = GetValueForHandlerParameter(symbol3, context);
                var value4 = GetValueForHandlerParameter(symbol4, context);
                var value5 = GetValueForHandlerParameter(symbol5, context);
                var value6 = GetValueForHandlerParameter(symbol6, context);
                var value7 = GetValueForHandlerParameter(symbol7, context);

                handle(value1!, value2!, value3!, value4!, value5!, value6!, value7!);
            });

    /// <summary>
    /// Sets a command's handler based on an <see cref="Action{T1,T2,T3,T4,T5,T6,T7,T8}"/>.
    /// </summary>
    public static void SetHandler<T1, T2, T3, T4, T5, T6, T7, T8>(
        this Command command,
        Action<T1, T2, T3, T4, T5, T6, T7, T8> handle,
        IValueDescriptor<T1> symbol1,
        IValueDescriptor<T2> symbol2,
        IValueDescriptor<T3> symbol3,
        IValueDescriptor<T4> symbol4,
        IValueDescriptor<T5> symbol5,
        IValueDescriptor<T6> symbol6,
        IValueDescriptor<T7> symbol7,
        IValueDescriptor<T8> symbol8) =>
        command.Handler = new AnonymousCommandHandler(
            context =>
            {
                var value1 = GetValueForHandlerParameter(symbol1, context);
                var value2 = GetValueForHandlerParameter(symbol2, context);
                var value3 = GetValueForHandlerParameter(symbol3, context);
                var value4 = GetValueForHandlerParameter(symbol4, context);
                var value5 = GetValueForHandlerParameter(symbol5, context);
                var value6 = GetValueForHandlerParameter(symbol6, context);
                var value7 = GetValueForHandlerParameter(symbol7, context);
                var value8 = GetValueForHandlerParameter(symbol8, context);

                handle(value1!, value2!, value3!, value4!, value5!, value6!, value7!, value8!);
            });
}