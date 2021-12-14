// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    /// <summary>
    /// Provides methods for creating and working with command handlers.
    /// </summary>
    public static partial class CommandHandler
    {
        /// <summary>
        /// Creates a command handler based on a <see cref="Func{T}"/>.
        /// </summary>
        public static ICommandHandler SetHandler(
            this Command command,
            Func<Task> handle) =>
            command.Handler = new AnonymousCommandHandler(_ => handle());

        /// <summary>
        /// Creates a command handler based on a <see cref="Func{T}"/>.
        /// </summary>
        public static ICommandHandler SetHandler<T>(
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
        /// Creates a command handler based on a <see cref="Func{T1,T2}"/>.
        /// </summary>
        public static ICommandHandler SetHandler<T1, T2>(
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
        /// Creates a command handler based on a <see cref="Func{T1,T2,T3}"/>.
        /// </summary>
        public static ICommandHandler SetHandler<T1, T2, T3>(
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
        /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4}"/>.
        /// </summary>
        public static ICommandHandler SetHandler<T1, T2, T3, T4>(
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
        /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5}"/>.
        /// </summary>
        public static ICommandHandler SetHandler<T1, T2, T3, T4, T5>(
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
        /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6}"/>.
        /// </summary>
        public static ICommandHandler SetHandler<T1, T2, T3, T4, T5, T6>(
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
        /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7}"/>.
        /// </summary>
        public static ICommandHandler SetHandler<T1, T2, T3, T4, T5, T6, T7>(
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
        /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8}"/>.
        /// </summary>
        public static ICommandHandler SetHandler<T1, T2, T3, T4, T5, T6, T7, T8>(
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
        /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9}"/>.
        /// </summary>
        public static ICommandHandler SetHandler<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
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
        /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10}"/>.
        /// </summary>
        public static ICommandHandler SetHandler<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
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
        /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11}"/>.
        /// </summary>
        public static ICommandHandler SetHandler<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
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
        /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12}"/>.
        /// </summary>
        public static ICommandHandler SetHandler<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
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
        /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13}"/>.
        /// </summary>
        public static ICommandHandler SetHandler<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
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
        /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14}"/>.
        /// </summary>
        public static ICommandHandler SetHandler<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
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
        /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15}"/>.
        /// </summary>
        public static ICommandHandler SetHandler<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
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
        /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16}"/>.
        /// </summary>
        public static ICommandHandler SetHandler<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(
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