// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;

namespace System.CommandLine.Invocation
{
    /// <summary>
    /// Provides methods for creating and working with command handlers.
    /// </summary>
    public static partial class CommandHandler
    {
        /// <summary>
        /// Creates a command handler based on a <see cref="Action{T}"/>.
        /// </summary>
        public static ICommandHandler Create(
            Action handle) =>
            new AnonymousCommandHandler(_ => handle());

        /// <summary>
        /// Creates a command handler based on a <see cref="Action{T}"/>.
        /// </summary>
        public static ICommandHandler Create<T>(
            IValueDescriptor<T> symbol1,
            Action<T> handle) =>
            new AnonymousCommandHandler(
                context => handle(
                    context.ParseResult.GetValueFor(symbol1)!));

        /// <summary>
        /// Creates a command handler based on a <see cref="Action{T1,T2}"/>.
        /// </summary>
        public static ICommandHandler Create<T1, T2>(
            IValueDescriptor<T1> symbol1,
            IValueDescriptor<T2> symbol2,
            Action<T1, T2> handle) =>
            new AnonymousCommandHandler(
                context => handle(
                    context.ParseResult.GetValueFor(symbol1)!,
                    context.ParseResult.GetValueFor(symbol2)!));

        /// <summary>
        /// Creates a command handler based on a <see cref="Action{T1,T2,T3}"/>.
        /// </summary>
        public static ICommandHandler Create<T1, T2, T3>(
            IValueDescriptor<T1> symbol1,
            IValueDescriptor<T2> symbol2,
            IValueDescriptor<T3> symbol3,
            Action<T1, T2, T3> handle) =>
            new AnonymousCommandHandler(
                context => handle(
                    context.ParseResult.GetValueFor(symbol1)!,
                    context.ParseResult.GetValueFor(symbol2)!,
                    context.ParseResult.GetValueFor(symbol3)!));

        /// <summary>
        /// Creates a command handler based on a <see cref="Action{T1,T2,T3,T4}"/>.
        /// </summary>
        public static ICommandHandler Create<T1, T2, T3, T4>(
            IValueDescriptor<T1> symbol1,
            IValueDescriptor<T2> symbol2,
            IValueDescriptor<T3> symbol3,
            IValueDescriptor<T4> symbol4,
            Action<T1, T2, T3, T4> handle) =>
            new AnonymousCommandHandler(
                context => handle(
                    context.ParseResult.GetValueFor(symbol1)!,
                    context.ParseResult.GetValueFor(symbol2)!,
                    context.ParseResult.GetValueFor(symbol3)!,
                    context.ParseResult.GetValueFor(symbol4)!));

        /// <summary>
        /// Creates a command handler based on a <see cref="Action{T1,T2,T3,T4,T5}"/>.
        /// </summary>
        public static ICommandHandler Create<T1, T2, T3, T4, T5>(
            IValueDescriptor<T1> symbol1,
            IValueDescriptor<T2> symbol2,
            IValueDescriptor<T3> symbol3,
            IValueDescriptor<T4> symbol4,
            IValueDescriptor<T5> symbol5,
            Action<T1, T2, T3, T4, T5> handle) =>
            new AnonymousCommandHandler(
                context => handle(
                    context.ParseResult.GetValueFor(symbol1)!,
                    context.ParseResult.GetValueFor(symbol2)!,
                    context.ParseResult.GetValueFor(symbol3)!,
                    context.ParseResult.GetValueFor(symbol4)!,
                    context.ParseResult.GetValueFor(symbol5)!));

        /// <summary>
        /// Creates a command handler based on a <see cref="Action{T1,T2,T3,T4,T5,T6}"/>.
        /// </summary>
        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6>(
            IValueDescriptor<T1> symbol1,
            IValueDescriptor<T2> symbol2,
            IValueDescriptor<T3> symbol3,
            IValueDescriptor<T4> symbol4,
            IValueDescriptor<T5> symbol5,
            IValueDescriptor<T6> symbol6,
            Action<T1, T2, T3, T4, T5, T6> handle) =>
            new AnonymousCommandHandler(
                context => handle(
                    context.ParseResult.GetValueFor(symbol1)!,
                    context.ParseResult.GetValueFor(symbol2)!,
                    context.ParseResult.GetValueFor(symbol3)!,
                    context.ParseResult.GetValueFor(symbol4)!,
                    context.ParseResult.GetValueFor(symbol5)!,
                    context.ParseResult.GetValueFor(symbol6)!));

        /// <summary>
        /// Creates a command handler based on a <see cref="Action{T1,T2,T3,T4,T5,T6,T7}"/>.
        /// </summary>
        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7>(
            IValueDescriptor<T1> symbol1,
            IValueDescriptor<T2> symbol2,
            IValueDescriptor<T3> symbol3,
            IValueDescriptor<T4> symbol4,
            IValueDescriptor<T5> symbol5,
            IValueDescriptor<T6> symbol6,
            IValueDescriptor<T7> symbol7,
            Action<T1, T2, T3, T4, T5, T6, T7> handle) =>
            new AnonymousCommandHandler(
                context => handle(
                    context.ParseResult.GetValueFor(symbol1)!,
                    context.ParseResult.GetValueFor(symbol2)!,
                    context.ParseResult.GetValueFor(symbol3)!,
                    context.ParseResult.GetValueFor(symbol4)!,
                    context.ParseResult.GetValueFor(symbol5)!,
                    context.ParseResult.GetValueFor(symbol6)!,
                    context.ParseResult.GetValueFor(symbol7)!));

        /// <summary>
        /// Creates a command handler based on a <see cref="Action{T1,T2,T3,T4,T5,T6,T7,T8}"/>.
        /// </summary>
        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8>(
            IValueDescriptor<T1> symbol1,
            IValueDescriptor<T2> symbol2,
            IValueDescriptor<T3> symbol3,
            IValueDescriptor<T4> symbol4,
            IValueDescriptor<T5> symbol5,
            IValueDescriptor<T6> symbol6,
            IValueDescriptor<T7> symbol7,
            IValueDescriptor<T8> symbol8,
            Action<T1, T2, T3, T4, T5, T6, T7, T8> handle) =>
            new AnonymousCommandHandler(
                context => handle(
                    context.ParseResult.GetValueFor(symbol1)!,
                    context.ParseResult.GetValueFor(symbol2)!,
                    context.ParseResult.GetValueFor(symbol3)!,
                    context.ParseResult.GetValueFor(symbol4)!,
                    context.ParseResult.GetValueFor(symbol5)!,
                    context.ParseResult.GetValueFor(symbol6)!,
                    context.ParseResult.GetValueFor(symbol7)!,
                    context.ParseResult.GetValueFor(symbol8)!));

        /// <summary>
        /// Creates a command handler based on a <see cref="Action{T1,T2,T3,T4,T5,T6,T7,T8,T9}"/>.
        /// </summary>
        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            IValueDescriptor<T1> symbol1,
            IValueDescriptor<T2> symbol2,
            IValueDescriptor<T3> symbol3,
            IValueDescriptor<T4> symbol4,
            IValueDescriptor<T5> symbol5,
            IValueDescriptor<T6> symbol6,
            IValueDescriptor<T7> symbol7,
            IValueDescriptor<T8> symbol8,
            IValueDescriptor<T9> symbol9,
            Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> handle) =>
            new AnonymousCommandHandler(
                context => handle(
                    context.ParseResult.GetValueFor(symbol1)!,
                    context.ParseResult.GetValueFor(symbol2)!,
                    context.ParseResult.GetValueFor(symbol3)!,
                    context.ParseResult.GetValueFor(symbol4)!,
                    context.ParseResult.GetValueFor(symbol5)!,
                    context.ParseResult.GetValueFor(symbol6)!,
                    context.ParseResult.GetValueFor(symbol7)!,
                    context.ParseResult.GetValueFor(symbol8)!,
                    context.ParseResult.GetValueFor(symbol9)!));

        /// <summary>
        /// Creates a command handler based on a <see cref="Action{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10}"/>.
        /// </summary>
        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            IValueDescriptor<T1> symbol1,
            IValueDescriptor<T2> symbol2,
            IValueDescriptor<T3> symbol3,
            IValueDescriptor<T4> symbol4,
            IValueDescriptor<T5> symbol5,
            IValueDescriptor<T6> symbol6,
            IValueDescriptor<T7> symbol7,
            IValueDescriptor<T8> symbol8,
            IValueDescriptor<T9> symbol9,
            IValueDescriptor<T10> symbol10,
            Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> handle) =>
            new AnonymousCommandHandler(
                context => handle(
                    context.ParseResult.GetValueFor(symbol1)!,
                    context.ParseResult.GetValueFor(symbol2)!,
                    context.ParseResult.GetValueFor(symbol3)!,
                    context.ParseResult.GetValueFor(symbol4)!,
                    context.ParseResult.GetValueFor(symbol5)!,
                    context.ParseResult.GetValueFor(symbol6)!,
                    context.ParseResult.GetValueFor(symbol7)!,
                    context.ParseResult.GetValueFor(symbol8)!,
                    context.ParseResult.GetValueFor(symbol9)!,
                    context.ParseResult.GetValueFor(symbol10)!));

        /// <summary>
        /// Creates a command handler based on a <see cref="Action{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11}"/>.
        /// </summary>
        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
            IValueDescriptor<T1> symbol1,
            IValueDescriptor<T2> symbol2,
            IValueDescriptor<T3> symbol3,
            IValueDescriptor<T4> symbol4,
            IValueDescriptor<T5> symbol5,
            IValueDescriptor<T6> symbol6,
            IValueDescriptor<T7> symbol7,
            IValueDescriptor<T8> symbol8,
            IValueDescriptor<T9> symbol9,
            IValueDescriptor<T10> symbol10,
            IValueDescriptor<T11> symbol11,
            Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> handle) =>
            new AnonymousCommandHandler(
                context => handle(
                    context.ParseResult.GetValueFor(symbol1)!,
                    context.ParseResult.GetValueFor(symbol2)!,
                    context.ParseResult.GetValueFor(symbol3)!,
                    context.ParseResult.GetValueFor(symbol4)!,
                    context.ParseResult.GetValueFor(symbol5)!,
                    context.ParseResult.GetValueFor(symbol6)!,
                    context.ParseResult.GetValueFor(symbol7)!,
                    context.ParseResult.GetValueFor(symbol8)!,
                    context.ParseResult.GetValueFor(symbol9)!,
                    context.ParseResult.GetValueFor(symbol10)!,
                    context.ParseResult.GetValueFor(symbol11)!));

        /// <summary>
        /// Creates a command handler based on a <see cref="Action{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12}"/>.
        /// </summary>
        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
            IValueDescriptor<T1> symbol1,
            IValueDescriptor<T2> symbol2,
            IValueDescriptor<T3> symbol3,
            IValueDescriptor<T4> symbol4,
            IValueDescriptor<T5> symbol5,
            IValueDescriptor<T6> symbol6,
            IValueDescriptor<T7> symbol7,
            IValueDescriptor<T8> symbol8,
            IValueDescriptor<T9> symbol9,
            IValueDescriptor<T10> symbol10,
            IValueDescriptor<T11> symbol11,
            IValueDescriptor<T12> symbol12,
            Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> handle) =>
            new AnonymousCommandHandler(
                context => handle(
                    context.ParseResult.GetValueFor(symbol1)!,
                    context.ParseResult.GetValueFor(symbol2)!,
                    context.ParseResult.GetValueFor(symbol3)!,
                    context.ParseResult.GetValueFor(symbol4)!,
                    context.ParseResult.GetValueFor(symbol5)!,
                    context.ParseResult.GetValueFor(symbol6)!,
                    context.ParseResult.GetValueFor(symbol7)!,
                    context.ParseResult.GetValueFor(symbol8)!,
                    context.ParseResult.GetValueFor(symbol9)!,
                    context.ParseResult.GetValueFor(symbol10)!,
                    context.ParseResult.GetValueFor(symbol11)!,
                    context.ParseResult.GetValueFor(symbol12)!));

        /// <summary>
        /// Creates a command handler based on a <see cref="Action{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13}"/>.
        /// </summary>
        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
            IValueDescriptor<T1> symbol1,
            IValueDescriptor<T2> symbol2,
            IValueDescriptor<T3> symbol3,
            IValueDescriptor<T4> symbol4,
            IValueDescriptor<T5> symbol5,
            IValueDescriptor<T6> symbol6,
            IValueDescriptor<T7> symbol7,
            IValueDescriptor<T8> symbol8,
            IValueDescriptor<T9> symbol9,
            IValueDescriptor<T10> symbol10,
            IValueDescriptor<T11> symbol11,
            IValueDescriptor<T12> symbol12,
            IValueDescriptor<T13> symbol13,
            Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> handle) =>
            new AnonymousCommandHandler(
                context => handle(
                    context.ParseResult.GetValueFor(symbol1)!,
                    context.ParseResult.GetValueFor(symbol2)!,
                    context.ParseResult.GetValueFor(symbol3)!,
                    context.ParseResult.GetValueFor(symbol4)!,
                    context.ParseResult.GetValueFor(symbol5)!,
                    context.ParseResult.GetValueFor(symbol6)!,
                    context.ParseResult.GetValueFor(symbol7)!,
                    context.ParseResult.GetValueFor(symbol8)!,
                    context.ParseResult.GetValueFor(symbol9)!,
                    context.ParseResult.GetValueFor(symbol10)!,
                    context.ParseResult.GetValueFor(symbol11)!,
                    context.ParseResult.GetValueFor(symbol12)!,
                    context.ParseResult.GetValueFor(symbol13)!));

        /// <summary>
        /// Creates a command handler based on a <see cref="Action{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14}"/>.
        /// </summary>
        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
            IValueDescriptor<T1> symbol1,
            IValueDescriptor<T2> symbol2,
            IValueDescriptor<T3> symbol3,
            IValueDescriptor<T4> symbol4,
            IValueDescriptor<T5> symbol5,
            IValueDescriptor<T6> symbol6,
            IValueDescriptor<T7> symbol7,
            IValueDescriptor<T8> symbol8,
            IValueDescriptor<T9> symbol9,
            IValueDescriptor<T10> symbol10,
            IValueDescriptor<T11> symbol11,
            IValueDescriptor<T12> symbol12,
            IValueDescriptor<T13> symbol13,
            IValueDescriptor<T14> symbol14,
            Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> handle) =>
            new AnonymousCommandHandler(
                context => handle(
                    context.ParseResult.GetValueFor(symbol1)!,
                    context.ParseResult.GetValueFor(symbol2)!,
                    context.ParseResult.GetValueFor(symbol3)!,
                    context.ParseResult.GetValueFor(symbol4)!,
                    context.ParseResult.GetValueFor(symbol5)!,
                    context.ParseResult.GetValueFor(symbol6)!,
                    context.ParseResult.GetValueFor(symbol7)!,
                    context.ParseResult.GetValueFor(symbol8)!,
                    context.ParseResult.GetValueFor(symbol9)!,
                    context.ParseResult.GetValueFor(symbol10)!,
                    context.ParseResult.GetValueFor(symbol11)!,
                    context.ParseResult.GetValueFor(symbol12)!,
                    context.ParseResult.GetValueFor(symbol13)!,
                    context.ParseResult.GetValueFor(symbol14)!));

        /// <summary>
        /// Creates a command handler based on a <see cref="Action{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15}"/>.
        /// </summary>
        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
            IValueDescriptor<T1> symbol1,
            IValueDescriptor<T2> symbol2,
            IValueDescriptor<T3> symbol3,
            IValueDescriptor<T4> symbol4,
            IValueDescriptor<T5> symbol5,
            IValueDescriptor<T6> symbol6,
            IValueDescriptor<T7> symbol7,
            IValueDescriptor<T8> symbol8,
            IValueDescriptor<T9> symbol9,
            IValueDescriptor<T10> symbol10,
            IValueDescriptor<T11> symbol11,
            IValueDescriptor<T12> symbol12,
            IValueDescriptor<T13> symbol13,
            IValueDescriptor<T14> symbol14,
            IValueDescriptor<T15> symbol15,
            Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> handle) =>
            new AnonymousCommandHandler(
                context => handle(
                    context.ParseResult.GetValueFor(symbol1)!,
                    context.ParseResult.GetValueFor(symbol2)!,
                    context.ParseResult.GetValueFor(symbol3)!,
                    context.ParseResult.GetValueFor(symbol4)!,
                    context.ParseResult.GetValueFor(symbol5)!,
                    context.ParseResult.GetValueFor(symbol6)!,
                    context.ParseResult.GetValueFor(symbol7)!,
                    context.ParseResult.GetValueFor(symbol8)!,
                    context.ParseResult.GetValueFor(symbol9)!,
                    context.ParseResult.GetValueFor(symbol10)!,
                    context.ParseResult.GetValueFor(symbol11)!,
                    context.ParseResult.GetValueFor(symbol12)!,
                    context.ParseResult.GetValueFor(symbol13)!,
                    context.ParseResult.GetValueFor(symbol14)!,
                    context.ParseResult.GetValueFor(symbol15)!));

        /// <summary>
        /// Creates a command handler based on a <see cref="Action{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16}"/>.
        /// </summary>
        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(
            IValueDescriptor<T1> symbol1,
            IValueDescriptor<T2> symbol2,
            IValueDescriptor<T3> symbol3,
            IValueDescriptor<T4> symbol4,
            IValueDescriptor<T5> symbol5,
            IValueDescriptor<T6> symbol6,
            IValueDescriptor<T7> symbol7,
            IValueDescriptor<T8> symbol8,
            IValueDescriptor<T9> symbol9,
            IValueDescriptor<T10> symbol10,
            IValueDescriptor<T11> symbol11,
            IValueDescriptor<T12> symbol12,
            IValueDescriptor<T13> symbol13,
            IValueDescriptor<T14> symbol14,
            IValueDescriptor<T15> symbol15,
            IValueDescriptor<T16> symbol16,
            Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> handle) =>
            new AnonymousCommandHandler(
                context => handle(
                    context.ParseResult.GetValueFor(symbol1)!,
                    context.ParseResult.GetValueFor(symbol2)!,
                    context.ParseResult.GetValueFor(symbol3)!,
                    context.ParseResult.GetValueFor(symbol4)!,
                    context.ParseResult.GetValueFor(symbol5)!,
                    context.ParseResult.GetValueFor(symbol6)!,
                    context.ParseResult.GetValueFor(symbol7)!,
                    context.ParseResult.GetValueFor(symbol8)!,
                    context.ParseResult.GetValueFor(symbol9)!,
                    context.ParseResult.GetValueFor(symbol10)!,
                    context.ParseResult.GetValueFor(symbol11)!,
                    context.ParseResult.GetValueFor(symbol12)!,
                    context.ParseResult.GetValueFor(symbol13)!,
                    context.ParseResult.GetValueFor(symbol14)!,
                    context.ParseResult.GetValueFor(symbol15)!,
                    context.ParseResult.GetValueFor(symbol16)!));
    }
}