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
        params IValueDescriptor[] symbols) =>
        command.Handler = new AnonymousCommandHandler(
            context =>
            {
                var index = 0;

                var value1 = GetValueForHandlerParameter<T>(symbols, ref index, context);

                handle(value1!);
            });

    /// <summary>
    /// Sets a command's handler based on an <see cref="Action{T1,T2}"/>.
    /// </summary>
    public static void SetHandler<T1, T2>(
        this Command command,
        Action<T1, T2> handle,
        params IValueDescriptor[] symbols) =>
        command.Handler = new AnonymousCommandHandler(
            context =>
            {
                var index = 0;

                var value1 = GetValueForHandlerParameter<T1>(symbols, ref index, context);
                var value2 = GetValueForHandlerParameter<T2>(symbols, ref index, context);

                handle(value1!, value2!);
            });

    /// <summary>
    /// Sets a command's handler based on an <see cref="Action{T1,T2,T3}"/>.
    /// </summary>
    public static void SetHandler<T1, T2, T3>(
        this Command command,
        Action<T1, T2, T3> handle,
        params IValueDescriptor[] symbols) =>
        command.Handler = new AnonymousCommandHandler(
            context =>
            {
                var index = 0;

                var value1 = GetValueForHandlerParameter<T1>(symbols, ref index, context);
                var value2 = GetValueForHandlerParameter<T2>(symbols, ref index, context);
                var value3 = GetValueForHandlerParameter<T3>(symbols, ref index, context);

                handle(value1!, value2!, value3!);
            });

    /// <summary>
    /// Sets a command's handler based on an <see cref="Action{T1,T2,T3,T4}"/>.
    /// </summary>
    public static void SetHandler<T1, T2, T3, T4>(
        this Command command,
        Action<T1, T2, T3, T4> handle,
        params IValueDescriptor[] symbols) =>
        command.Handler = new AnonymousCommandHandler(
            context =>
            {
                var index = 0;

                var value1 = GetValueForHandlerParameter<T1>(symbols, ref index, context);
                var value2 = GetValueForHandlerParameter<T2>(symbols, ref index, context);
                var value3 = GetValueForHandlerParameter<T3>(symbols, ref index, context);
                var value4 = GetValueForHandlerParameter<T4>(symbols, ref index, context);

                handle(value1!, value2!, value3!, value4!);
            });

    /// <summary>
    /// Sets a command's handler based on an <see cref="Action{T1,T2,T3,T4,T5}"/>.
    /// </summary>
    public static void SetHandler<T1, T2, T3, T4, T5>(
        this Command command,
        Action<T1, T2, T3, T4, T5> handle,
        params IValueDescriptor[] symbols) =>
        command.Handler = new AnonymousCommandHandler(
            context =>
            {
                var index = 0;

                var value1 = GetValueForHandlerParameter<T1>(symbols, ref index, context);
                var value2 = GetValueForHandlerParameter<T2>(symbols, ref index, context);
                var value3 = GetValueForHandlerParameter<T3>(symbols, ref index, context);
                var value4 = GetValueForHandlerParameter<T4>(symbols, ref index, context);
                var value5 = GetValueForHandlerParameter<T5>(symbols, ref index, context);

                handle(value1!, value2!, value3!, value4!, value5!);
            });

    /// <summary>
    /// Sets a command's handler based on an <see cref="Action{T1,T2,T3,T4,T5,T6}"/>.
    /// </summary>
    public static void SetHandler<T1, T2, T3, T4, T5, T6>(
        this Command command,
        Action<T1, T2, T3, T4, T5, T6> handle,
        params IValueDescriptor[] symbols) =>
        command.Handler = new AnonymousCommandHandler(
            context =>
            {
                var index = 0;

                var value1 = GetValueForHandlerParameter<T1>(symbols, ref index, context);
                var value2 = GetValueForHandlerParameter<T2>(symbols, ref index, context);
                var value3 = GetValueForHandlerParameter<T3>(symbols, ref index, context);
                var value4 = GetValueForHandlerParameter<T4>(symbols, ref index, context);
                var value5 = GetValueForHandlerParameter<T5>(symbols, ref index, context);
                var value6 = GetValueForHandlerParameter<T6>(symbols, ref index, context);

                handle(value1!, value2!, value3!, value4!, value5!, value6!);
            });

    /// <summary>
    /// Sets a command's handler based on an <see cref="Action{T1,T2,T3,T4,T5,T6,T7}"/>.
    /// </summary>
    public static void SetHandler<T1, T2, T3, T4, T5, T6, T7>(
        this Command command,
        Action<T1, T2, T3, T4, T5, T6, T7> handle,
        params IValueDescriptor[] symbols) =>
        command.Handler = new AnonymousCommandHandler(
            context =>
            {
                var index = 0;

                var value1 = GetValueForHandlerParameter<T1>(symbols, ref index, context);
                var value2 = GetValueForHandlerParameter<T2>(symbols, ref index, context);
                var value3 = GetValueForHandlerParameter<T3>(symbols, ref index, context);
                var value4 = GetValueForHandlerParameter<T4>(symbols, ref index, context);
                var value5 = GetValueForHandlerParameter<T5>(symbols, ref index, context);
                var value6 = GetValueForHandlerParameter<T6>(symbols, ref index, context);
                var value7 = GetValueForHandlerParameter<T7>(symbols, ref index, context);

                handle(value1!, value2!, value3!, value4!, value5!, value6!, value7!);
            });

    /// <summary>
    /// Sets a command's handler based on an <see cref="Action{T1,T2,T3,T4,T5,T6,T7,T8}"/>.
    /// </summary>
    public static void SetHandler<T1, T2, T3, T4, T5, T6, T7, T8>(
        this Command command,
        Action<T1, T2, T3, T4, T5, T6, T7, T8> handle,
        params IValueDescriptor[] symbols) =>
        command.Handler = new AnonymousCommandHandler(
            context =>
            {
                var index = 0;

                var value1 = GetValueForHandlerParameter<T1>(symbols, ref index, context);
                var value2 = GetValueForHandlerParameter<T2>(symbols, ref index, context);
                var value3 = GetValueForHandlerParameter<T3>(symbols, ref index, context);
                var value4 = GetValueForHandlerParameter<T4>(symbols, ref index, context);
                var value5 = GetValueForHandlerParameter<T5>(symbols, ref index, context);
                var value6 = GetValueForHandlerParameter<T6>(symbols, ref index, context);
                var value7 = GetValueForHandlerParameter<T7>(symbols, ref index, context);
                var value8 = GetValueForHandlerParameter<T8>(symbols, ref index, context);

                handle(value1!, value2!, value3!, value4!, value5!, value6!, value7!, value8!);
            });
}