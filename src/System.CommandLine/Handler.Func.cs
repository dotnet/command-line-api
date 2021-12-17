// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace System.CommandLine
{
    /// <summary>
    /// Provides methods for creating and working with command handlers.
    /// </summary>
    public static partial class Handler
    {
        /// <summary>
        /// Sets a command's handler based on a <see cref="Func{Task}"/>.
        /// </summary>
        public static void SetHandler(
            this Command command,
            Func<Task> handle) =>
            command.Handler = new AnonymousCommandHandler(_ => handle());

        /// <summary>
        /// Sets a command's handler based on a <see cref="Func{T,Task}"/>.
        /// </summary>
        public static void SetHandler<T>(
            this Command command,
            Func<T, Task> handle,
            params IValueDescriptor[] symbols) =>
            command.Handler = new AnonymousCommandHandler(
                context =>
                {
                    var index = 0;

                    var value1 = GetValueForHandlerParameter<T>(symbols, ref index, context);

                    return handle(value1!);
                });

        /// <summary>
        /// Sets a command's handler based on a <see cref="Func{T1,T2,Task}"/>.
        /// </summary>
        public static void SetHandler<T1, T2>(
            this Command command,
            Func<T1, T2, Task> handle,
            params IValueDescriptor[] symbols) =>
            command.Handler = new AnonymousCommandHandler(
                context =>
                {
                    var index = 0;

                    var value1 = GetValueForHandlerParameter<T1>(symbols, ref index, context);
                    var value2 = GetValueForHandlerParameter<T2>(symbols, ref index, context);

                    return handle(value1!, value2!);
                });

        /// <summary>
        /// Sets a command's handler based on a <see cref="Func{T1,T2,T3,Task}"/>.
        /// </summary>
        public static void SetHandler<T1, T2, T3>(
            this Command command,
            Func<T1, T2, T3, Task> handle,
            params IValueDescriptor[] symbols) =>
            command.Handler = new AnonymousCommandHandler(
                context =>
                {
                    var index = 0;

                    var value1 = GetValueForHandlerParameter<T1>(symbols, ref index, context);
                    var value2 = GetValueForHandlerParameter<T2>(symbols, ref index, context);
                    var value3 = GetValueForHandlerParameter<T3>(symbols, ref index, context);

                    return handle(value1!, value2!, value3!);
                });

        /// <summary>
        /// Sets a command's handler based on a <see cref="Func{T1,T2,T3,T4,Task}"/>.
        /// </summary>
        public static void SetHandler<T1, T2, T3, T4>(
            this Command command,
            Func<T1, T2, T3, T4, Task> handle,
            params IValueDescriptor[] symbols) =>
            command.Handler = new AnonymousCommandHandler(
                context =>
                {
                    var index = 0;

                    var value1 = GetValueForHandlerParameter<T1>(symbols, ref index, context);
                    var value2 = GetValueForHandlerParameter<T2>(symbols, ref index, context);
                    var value3 = GetValueForHandlerParameter<T3>(symbols, ref index, context);
                    var value4 = GetValueForHandlerParameter<T4>(symbols, ref index, context);

                    return handle(value1!, value2!, value3!, value4!);
                });

        /// <summary>
        /// Sets a command's handler based on a <see cref="Func{T1,T2,T3,T4,T5,Task}"/>.
        /// </summary>
        public static void SetHandler<T1, T2, T3, T4, T5>(
            this Command command,
            Func<T1, T2, T3, T4, T5, Task> handle,
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

                    return handle(value1!, value2!, value3!, value4!, value5!);
                });

        /// <summary>
        /// Sets a command's handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,Task}"/>.
        /// </summary>
        public static void SetHandler<T1, T2, T3, T4, T5, T6>(
            this Command command,
            Func<T1, T2, T3, T4, T5, T6, Task> handle,
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

                    return handle(value1!, value2!, value3!, value4!, value5!, value6!);
                });

        /// <summary>
        /// Sets a command's handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,Task}"/>.
        /// </summary>
        public static void SetHandler<T1, T2, T3, T4, T5, T6, T7>(
            this Command command,
            Func<T1, T2, T3, T4, T5, T6, T7, Task> handle,
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

                    return handle(value1!, value2!, value3!, value4!, value5!, value6!, value7!);
                });

        /// <summary>
        /// Sets a command's handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,Task}"/>.
        /// </summary>
        public static void SetHandler<T1, T2, T3, T4, T5, T6, T7, T8>(
            this Command command,
            Func<T1, T2, T3, T4, T5, T6, T7, T8, Task> handle,
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

                    return handle(value1!, value2!, value3!, value4!, value5!, value6!, value7!, value8!);
                });

        /// <summary>
        /// Sets a command's handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,Task}"/>.
        /// </summary>
        public static void SetHandler<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            this Command command,
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, Task> handle,
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
                    var value9 = GetValueForHandlerParameter<T9>(symbols, ref index, context);

                    return handle(value1!, value2!, value3!, value4!, value5!, value6!, value7!, value8!, value9!);
                });

        /// <summary>
        /// Sets a command's handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,Task}"/>.
        /// </summary>
        public static void SetHandler<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            this Command command,
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, Task> handle,
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
                    var value9 = GetValueForHandlerParameter<T9>(symbols, ref index, context);
                    var value10 = GetValueForHandlerParameter<T10>(symbols, ref index, context);

                    return handle(value1!, value2!, value3!, value4!, value5!, value6!, value7!, value8!, value9!, value10!);
                });

        /// <summary>
        /// Sets a command's handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,Task}"/>.
        /// </summary>
        public static void SetHandler<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
            this Command command,
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, Task> handle,
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
                    var value9 = GetValueForHandlerParameter<T9>(symbols, ref index, context);
                    var value10 = GetValueForHandlerParameter<T10>(symbols, ref index, context);
                    var value11 = GetValueForHandlerParameter<T11>(symbols, ref index, context);

                    return handle(value1!, value2!, value3!, value4!, value5!, value6!, value7!, value8!, value9!, value10!, value11!);
                });

        /// <summary>
        /// Sets a command's handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,Task}"/>.
        /// </summary>
        public static void SetHandler<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
            this Command command,
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, Task> handle,
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
                    var value9 = GetValueForHandlerParameter<T9>(symbols, ref index, context);
                    var value10 = GetValueForHandlerParameter<T10>(symbols, ref index, context);
                    var value11 = GetValueForHandlerParameter<T11>(symbols, ref index, context);
                    var value12 = GetValueForHandlerParameter<T12>(symbols, ref index, context);

                    return handle(value1!, value2!, value3!, value4!, value5!, value6!, value7!, value8!, value9!, value10!, value11!, value12!);
                });

        /// <summary>
        /// Sets a command's handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,Task}"/>.
        /// </summary>
        public static void SetHandler<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
            this Command command,
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, Task> handle,
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
                    var value9 = GetValueForHandlerParameter<T9>(symbols, ref index, context);
                    var value10 = GetValueForHandlerParameter<T10>(symbols, ref index, context);
                    var value11 = GetValueForHandlerParameter<T11>(symbols, ref index, context);
                    var value12 = GetValueForHandlerParameter<T12>(symbols, ref index, context);
                    var value13 = GetValueForHandlerParameter<T13>(symbols, ref index, context);

                    return handle(value1!, value2!, value3!, value4!, value5!, value6!, value7!, value8!, value9!, value10!, value11!, value12!, value13!);
                });

        /// <summary>
        /// Sets a command's handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,Task}"/>.
        /// </summary>
        public static void SetHandler<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
            this Command command,
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, Task> handle,
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
                    var value9 = GetValueForHandlerParameter<T9>(symbols, ref index, context);
                    var value10 = GetValueForHandlerParameter<T10>(symbols, ref index, context);
                    var value11 = GetValueForHandlerParameter<T11>(symbols, ref index, context);
                    var value12 = GetValueForHandlerParameter<T12>(symbols, ref index, context);
                    var value13 = GetValueForHandlerParameter<T13>(symbols, ref index, context);
                    var value14 = GetValueForHandlerParameter<T14>(symbols, ref index, context);

                    return handle(value1!, value2!, value3!, value4!, value5!, value6!, value7!, value8!, value9!, value10!, value11!, value12!, value13!, value14!);
                });

        /// <summary>
        /// Sets a command's handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,Task}"/>.
        /// </summary>
        public static void SetHandler<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
            this Command command,
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, Task> handle,
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
                    var value9 = GetValueForHandlerParameter<T9>(symbols, ref index, context);
                    var value10 = GetValueForHandlerParameter<T10>(symbols, ref index, context);
                    var value11 = GetValueForHandlerParameter<T11>(symbols, ref index, context);
                    var value12 = GetValueForHandlerParameter<T12>(symbols, ref index, context);
                    var value13 = GetValueForHandlerParameter<T13>(symbols, ref index, context);
                    var value14 = GetValueForHandlerParameter<T14>(symbols, ref index, context);
                    var value15 = GetValueForHandlerParameter<T15>(symbols, ref index, context);

                    return handle(value1!, value2!, value3!, value4!, value5!, value6!, value7!, value8!, value9!, value10!, value11!, value12!, value13!, value14!, value15!);
                });

        /// <summary>
        /// Sets a command's handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,Task}"/>.
        /// </summary>
        public static void SetHandler<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(
            this Command command,
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, Task> handle,
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
                    var value9 = GetValueForHandlerParameter<T9>(symbols, ref index, context);
                    var value10 = GetValueForHandlerParameter<T10>(symbols, ref index, context);
                    var value11 = GetValueForHandlerParameter<T11>(symbols, ref index, context);
                    var value12 = GetValueForHandlerParameter<T12>(symbols, ref index, context);
                    var value13 = GetValueForHandlerParameter<T13>(symbols, ref index, context);
                    var value14 = GetValueForHandlerParameter<T14>(symbols, ref index, context);
                    var value15 = GetValueForHandlerParameter<T15>(symbols, ref index, context);
                    var value16 = GetValueForHandlerParameter<T16>(symbols, ref index, context);

                    return handle(value1!, value2!, value3!, value4!, value5!, value6!, value7!, value8!, value9!, value10!, value11!, value12!, value13!, value14!, value15!, value16!);
                });
    }
}