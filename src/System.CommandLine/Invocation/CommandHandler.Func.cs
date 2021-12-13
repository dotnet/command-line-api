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
        public static ICommandHandler Create(
            Func<Task> handle) =>
            new AnonymousCommandHandler(_ => handle());

        /// <summary>
        /// Creates a command handler based on a <see cref="Func{T}"/>.
        /// </summary>
        public static ICommandHandler Create<T>(
            Func<T, Task> handle,
            params IValueDescriptor[] symbols) =>
            new AnonymousCommandHandler(
                async context =>
                {
                    var index = 0;

                    var value1 = GetParsedValueOrService<T>(symbols, ref index, context);

                    await handle(value1!);
                });

        /// <summary>
        /// Creates a command handler based on a <see cref="Func{T1,T2}"/>.
        /// </summary>
        public static ICommandHandler Create<T1, T2>(
            Func<T1, T2, Task> handle,
            params IValueDescriptor[] symbols) =>
            new AnonymousCommandHandler(
                async context =>
                {
                    var index = 0;

                    var value1 = GetParsedValueOrService<T1>(symbols, ref index, context);
                    var value2 = GetParsedValueOrService<T2>(symbols, ref index, context);

                    await handle(value1!, value2!);
                });

        /// <summary>
        /// Creates a command handler based on a <see cref="Func{T1,T2,T3}"/>.
        /// </summary>
        public static ICommandHandler Create<T1, T2, T3>(
            Func<T1, T2, T3, Task> handle,
            params IValueDescriptor[] symbols) =>
            new AnonymousCommandHandler(
                async context =>
                {
                    var index = 0;

                    var value1 = GetParsedValueOrService<T1>(symbols, ref index, context);
                    var value2 = GetParsedValueOrService<T2>(symbols, ref index, context);
                    var value3 = GetParsedValueOrService<T3>(symbols, ref index, context);

                    await handle(value1!, value2!, value3!);
                });

        /// <summary>
        /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4}"/>.
        /// </summary>
        public static ICommandHandler Create<T1, T2, T3, T4>(
            Func<T1, T2, T3, T4, Task> handle,
            params IValueDescriptor[] symbols) =>
            new AnonymousCommandHandler(
                async context =>
                {
                    var index = 0;

                    var value1 = GetParsedValueOrService<T1>(symbols, ref index, context);
                    var value2 = GetParsedValueOrService<T2>(symbols, ref index, context);
                    var value3 = GetParsedValueOrService<T3>(symbols, ref index, context);
                    var value4 = GetParsedValueOrService<T4>(symbols, ref index, context);

                    await handle(value1!, value2!, value3!, value4!);
                });

        /// <summary>
        /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5}"/>.
        /// </summary>
        public static ICommandHandler Create<T1, T2, T3, T4, T5>(
            Func<T1, T2, T3, T4, T5, Task> handle,
            params IValueDescriptor[] symbols) =>
            new AnonymousCommandHandler(
                async context =>
                {
                    var index = 0;

                    var value1 = GetParsedValueOrService<T1>(symbols, ref index, context);
                    var value2 = GetParsedValueOrService<T2>(symbols, ref index, context);
                    var value3 = GetParsedValueOrService<T3>(symbols, ref index, context);
                    var value4 = GetParsedValueOrService<T4>(symbols, ref index, context);
                    var value5 = GetParsedValueOrService<T5>(symbols, ref index, context);

                    await handle(value1!, value2!, value3!, value4!, value5!);
                });

        /// <summary>
        /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6}"/>.
        /// </summary>
        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6>(
            Func<T1, T2, T3, T4, T5, T6, Task> handle,
            params IValueDescriptor[] symbols) =>
            new AnonymousCommandHandler(
                async context =>
                {
                    var index = 0;

                    var value1 = GetParsedValueOrService<T1>(symbols, ref index, context);
                    var value2 = GetParsedValueOrService<T2>(symbols, ref index, context);
                    var value3 = GetParsedValueOrService<T3>(symbols, ref index, context);
                    var value4 = GetParsedValueOrService<T4>(symbols, ref index, context);
                    var value5 = GetParsedValueOrService<T5>(symbols, ref index, context);
                    var value6 = GetParsedValueOrService<T6>(symbols, ref index, context);

                    await handle(value1!, value2!, value3!, value4!, value5!, value6!);
                });

        /// <summary>
        /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7}"/>.
        /// </summary>
        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7>(
            Func<T1, T2, T3, T4, T5, T6, T7, Task> handle,
            params IValueDescriptor[] symbols) =>
            new AnonymousCommandHandler(
                async context =>
                {
                    var index = 0;

                    var value1 = GetParsedValueOrService<T1>(symbols, ref index, context);
                    var value2 = GetParsedValueOrService<T2>(symbols, ref index, context);
                    var value3 = GetParsedValueOrService<T3>(symbols, ref index, context);
                    var value4 = GetParsedValueOrService<T4>(symbols, ref index, context);
                    var value5 = GetParsedValueOrService<T5>(symbols, ref index, context);
                    var value6 = GetParsedValueOrService<T6>(symbols, ref index, context);
                    var value7 = GetParsedValueOrService<T7>(symbols, ref index, context);

                    await handle(value1!, value2!, value3!, value4!, value5!, value6!, value7!);
                });

        /// <summary>
        /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8}"/>.
        /// </summary>
        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8>(
            Func<T1, T2, T3, T4, T5, T6, T7, T8, Task> handle,
            params IValueDescriptor[] symbols) =>
            new AnonymousCommandHandler(
                async context =>
                {
                    var index = 0;

                    var value1 = GetParsedValueOrService<T1>(symbols, ref index, context);
                    var value2 = GetParsedValueOrService<T2>(symbols, ref index, context);
                    var value3 = GetParsedValueOrService<T3>(symbols, ref index, context);
                    var value4 = GetParsedValueOrService<T4>(symbols, ref index, context);
                    var value5 = GetParsedValueOrService<T5>(symbols, ref index, context);
                    var value6 = GetParsedValueOrService<T6>(symbols, ref index, context);
                    var value7 = GetParsedValueOrService<T7>(symbols, ref index, context);
                    var value8 = GetParsedValueOrService<T8>(symbols, ref index, context);

                    await handle(value1!, value2!, value3!, value4!, value5!, value6!, value7!, value8!);
                });

        /// <summary>
        /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9}"/>.
        /// </summary>
        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, Task> handle,
            params IValueDescriptor[] symbols) =>
            new AnonymousCommandHandler(
                async context =>
                {
                    var index = 0;

                    var value1 = GetParsedValueOrService<T1>(symbols, ref index, context);
                    var value2 = GetParsedValueOrService<T2>(symbols, ref index, context);
                    var value3 = GetParsedValueOrService<T3>(symbols, ref index, context);
                    var value4 = GetParsedValueOrService<T4>(symbols, ref index, context);
                    var value5 = GetParsedValueOrService<T5>(symbols, ref index, context);
                    var value6 = GetParsedValueOrService<T6>(symbols, ref index, context);
                    var value7 = GetParsedValueOrService<T7>(symbols, ref index, context);
                    var value8 = GetParsedValueOrService<T8>(symbols, ref index, context);
                    var value9 = GetParsedValueOrService<T9>(symbols, ref index, context);

                    await handle(value1!, value2!, value3!, value4!, value5!, value6!, value7!, value8!, value9!);
                });

        /// <summary>
        /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10}"/>.
        /// </summary>
        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, Task> handle,
            params IValueDescriptor[] symbols) =>
            new AnonymousCommandHandler(
                async context =>
                {
                    var index = 0;

                    var value1 = GetParsedValueOrService<T1>(symbols, ref index, context);
                    var value2 = GetParsedValueOrService<T2>(symbols, ref index, context);
                    var value3 = GetParsedValueOrService<T3>(symbols, ref index, context);
                    var value4 = GetParsedValueOrService<T4>(symbols, ref index, context);
                    var value5 = GetParsedValueOrService<T5>(symbols, ref index, context);
                    var value6 = GetParsedValueOrService<T6>(symbols, ref index, context);
                    var value7 = GetParsedValueOrService<T7>(symbols, ref index, context);
                    var value8 = GetParsedValueOrService<T8>(symbols, ref index, context);
                    var value9 = GetParsedValueOrService<T9>(symbols, ref index, context);
                    var value10 = GetParsedValueOrService<T10>(symbols, ref index, context);

                    await handle(value1!, value2!, value3!, value4!, value5!, value6!, value7!, value8!, value9!, value10!);
                });

        /// <summary>
        /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11}"/>.
        /// </summary>
        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, Task> handle,
            params IValueDescriptor[] symbols) =>
            new AnonymousCommandHandler(
                async context =>
                {
                    var index = 0;

                    var value1 = GetParsedValueOrService<T1>(symbols, ref index, context);
                    var value2 = GetParsedValueOrService<T2>(symbols, ref index, context);
                    var value3 = GetParsedValueOrService<T3>(symbols, ref index, context);
                    var value4 = GetParsedValueOrService<T4>(symbols, ref index, context);
                    var value5 = GetParsedValueOrService<T5>(symbols, ref index, context);
                    var value6 = GetParsedValueOrService<T6>(symbols, ref index, context);
                    var value7 = GetParsedValueOrService<T7>(symbols, ref index, context);
                    var value8 = GetParsedValueOrService<T8>(symbols, ref index, context);
                    var value9 = GetParsedValueOrService<T9>(symbols, ref index, context);
                    var value10 = GetParsedValueOrService<T10>(symbols, ref index, context);
                    var value11 = GetParsedValueOrService<T11>(symbols, ref index, context);

                    await handle(value1!, value2!, value3!, value4!, value5!, value6!, value7!, value8!, value9!, value10!, value11!);
                });

        /// <summary>
        /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12}"/>.
        /// </summary>
        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, Task> handle,
            params IValueDescriptor[] symbols) =>
            new AnonymousCommandHandler(
                async context =>
                {
                    var index = 0;

                    var value1 = GetParsedValueOrService<T1>(symbols, ref index, context);
                    var value2 = GetParsedValueOrService<T2>(symbols, ref index, context);
                    var value3 = GetParsedValueOrService<T3>(symbols, ref index, context);
                    var value4 = GetParsedValueOrService<T4>(symbols, ref index, context);
                    var value5 = GetParsedValueOrService<T5>(symbols, ref index, context);
                    var value6 = GetParsedValueOrService<T6>(symbols, ref index, context);
                    var value7 = GetParsedValueOrService<T7>(symbols, ref index, context);
                    var value8 = GetParsedValueOrService<T8>(symbols, ref index, context);
                    var value9 = GetParsedValueOrService<T9>(symbols, ref index, context);
                    var value10 = GetParsedValueOrService<T10>(symbols, ref index, context);
                    var value11 = GetParsedValueOrService<T11>(symbols, ref index, context);
                    var value12 = GetParsedValueOrService<T12>(symbols, ref index, context);

                    await handle(value1!, value2!, value3!, value4!, value5!, value6!, value7!, value8!, value9!, value10!, value11!, value12!);
                });

        /// <summary>
        /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13}"/>.
        /// </summary>
        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, Task> handle,
            params IValueDescriptor[] symbols) =>
            new AnonymousCommandHandler(
                async context =>
                {
                    var index = 0;

                    var value1 = GetParsedValueOrService<T1>(symbols, ref index, context);
                    var value2 = GetParsedValueOrService<T2>(symbols, ref index, context);
                    var value3 = GetParsedValueOrService<T3>(symbols, ref index, context);
                    var value4 = GetParsedValueOrService<T4>(symbols, ref index, context);
                    var value5 = GetParsedValueOrService<T5>(symbols, ref index, context);
                    var value6 = GetParsedValueOrService<T6>(symbols, ref index, context);
                    var value7 = GetParsedValueOrService<T7>(symbols, ref index, context);
                    var value8 = GetParsedValueOrService<T8>(symbols, ref index, context);
                    var value9 = GetParsedValueOrService<T9>(symbols, ref index, context);
                    var value10 = GetParsedValueOrService<T10>(symbols, ref index, context);
                    var value11 = GetParsedValueOrService<T11>(symbols, ref index, context);
                    var value12 = GetParsedValueOrService<T12>(symbols, ref index, context);
                    var value13 = GetParsedValueOrService<T13>(symbols, ref index, context);

                    await handle(value1!, value2!, value3!, value4!, value5!, value6!, value7!, value8!, value9!, value10!, value11!, value12!, value13!);
                });

        /// <summary>
        /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14}"/>.
        /// </summary>
        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, Task> handle,
            params IValueDescriptor[] symbols) =>
            new AnonymousCommandHandler(
                async context =>
                {
                    var index = 0;

                    var value1 = GetParsedValueOrService<T1>(symbols, ref index, context);
                    var value2 = GetParsedValueOrService<T2>(symbols, ref index, context);
                    var value3 = GetParsedValueOrService<T3>(symbols, ref index, context);
                    var value4 = GetParsedValueOrService<T4>(symbols, ref index, context);
                    var value5 = GetParsedValueOrService<T5>(symbols, ref index, context);
                    var value6 = GetParsedValueOrService<T6>(symbols, ref index, context);
                    var value7 = GetParsedValueOrService<T7>(symbols, ref index, context);
                    var value8 = GetParsedValueOrService<T8>(symbols, ref index, context);
                    var value9 = GetParsedValueOrService<T9>(symbols, ref index, context);
                    var value10 = GetParsedValueOrService<T10>(symbols, ref index, context);
                    var value11 = GetParsedValueOrService<T11>(symbols, ref index, context);
                    var value12 = GetParsedValueOrService<T12>(symbols, ref index, context);
                    var value13 = GetParsedValueOrService<T13>(symbols, ref index, context);
                    var value14 = GetParsedValueOrService<T14>(symbols, ref index, context);

                    await handle(value1!, value2!, value3!, value4!, value5!, value6!, value7!, value8!, value9!, value10!, value11!, value12!, value13!, value14!);
                });

        /// <summary>
        /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15}"/>.
        /// </summary>
        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, Task> handle,
            params IValueDescriptor[] symbols) =>
            new AnonymousCommandHandler(
                async context =>
                {
                    var index = 0;

                    var value1 = GetParsedValueOrService<T1>(symbols, ref index, context);
                    var value2 = GetParsedValueOrService<T2>(symbols, ref index, context);
                    var value3 = GetParsedValueOrService<T3>(symbols, ref index, context);
                    var value4 = GetParsedValueOrService<T4>(symbols, ref index, context);
                    var value5 = GetParsedValueOrService<T5>(symbols, ref index, context);
                    var value6 = GetParsedValueOrService<T6>(symbols, ref index, context);
                    var value7 = GetParsedValueOrService<T7>(symbols, ref index, context);
                    var value8 = GetParsedValueOrService<T8>(symbols, ref index, context);
                    var value9 = GetParsedValueOrService<T9>(symbols, ref index, context);
                    var value10 = GetParsedValueOrService<T10>(symbols, ref index, context);
                    var value11 = GetParsedValueOrService<T11>(symbols, ref index, context);
                    var value12 = GetParsedValueOrService<T12>(symbols, ref index, context);
                    var value13 = GetParsedValueOrService<T13>(symbols, ref index, context);
                    var value14 = GetParsedValueOrService<T14>(symbols, ref index, context);
                    var value15 = GetParsedValueOrService<T15>(symbols, ref index, context);

                    await handle(value1!, value2!, value3!, value4!, value5!, value6!, value7!, value8!, value9!, value10!, value11!, value12!, value13!, value14!, value15!);
                });

        /// <summary>
        /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16}"/>.
        /// </summary>
        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, Task> handle,
            params IValueDescriptor[] symbols) =>
            new AnonymousCommandHandler(
                async context =>
                {
                    var index = 0;

                    var value1 = GetParsedValueOrService<T1>(symbols, ref index, context);
                    var value2 = GetParsedValueOrService<T2>(symbols, ref index, context);
                    var value3 = GetParsedValueOrService<T3>(symbols, ref index, context);
                    var value4 = GetParsedValueOrService<T4>(symbols, ref index, context);
                    var value5 = GetParsedValueOrService<T5>(symbols, ref index, context);
                    var value6 = GetParsedValueOrService<T6>(symbols, ref index, context);
                    var value7 = GetParsedValueOrService<T7>(symbols, ref index, context);
                    var value8 = GetParsedValueOrService<T8>(symbols, ref index, context);
                    var value9 = GetParsedValueOrService<T9>(symbols, ref index, context);
                    var value10 = GetParsedValueOrService<T10>(symbols, ref index, context);
                    var value11 = GetParsedValueOrService<T11>(symbols, ref index, context);
                    var value12 = GetParsedValueOrService<T12>(symbols, ref index, context);
                    var value13 = GetParsedValueOrService<T13>(symbols, ref index, context);
                    var value14 = GetParsedValueOrService<T14>(symbols, ref index, context);
                    var value15 = GetParsedValueOrService<T15>(symbols, ref index, context);
                    var value16 = GetParsedValueOrService<T16>(symbols, ref index, context);

                    await handle(value1!, value2!, value3!, value4!, value5!, value6!, value7!, value8!, value9!, value10!, value11!, value12!, value13!, value14!, value15!, value16!);
                });
    }
}