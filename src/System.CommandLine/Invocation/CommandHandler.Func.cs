// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    public static partial class CommandHandler
    {
        /// <summary>
        /// Creates a command handler based on a <see cref="Func{Task}"/>.
        /// </summary>
        public static ICommandHandler Create(
            Func<Task> handle) =>
            new AnonymousCommandHandler(_ => handle());
        
        /// <summary>
        /// Creates a command handler based on a <see cref="Func{Task}"/>.
        /// </summary>
        public static ICommandHandler Create(
            Func<InvocationContext, Task> handle) =>
            new AnonymousCommandHandler(handle);

        /// <summary>
        /// Creates a command handler based on a <see cref="Func{T,Task}"/>.
        /// </summary>
        public static ICommandHandler Create<T>(
            IValueDescriptor<T> symbol1,
            Func<T, Task> handle) =>
            new AnonymousCommandHandler(
                async context => await handle(
                                     context.ParseResult.GetValueFor(symbol1)!));

        /// <summary>
        /// Creates a command handler based on a <see cref="Func{T1,T2,Task}"/>.
        /// </summary>
        public static ICommandHandler Create<T1, T2>(
            IValueDescriptor<T1> symbol1,
            IValueDescriptor<T2> symbol2,
            Func<T1, T2, Task> handle) =>
            new AnonymousCommandHandler(
                async context => await handle(
                                     context.ParseResult.GetValueFor(symbol1)!,
                                     context.ParseResult.GetValueFor(symbol2)!));

        /// <summary>
        /// Creates a command handler based on a <see cref="Func{T1,T2,T3,Task}"/>.
        /// </summary>
        public static ICommandHandler Create<T1, T2, T3>(
            IValueDescriptor<T1> symbol1,
            IValueDescriptor<T2> symbol2,
            IValueDescriptor<T3> symbol3,
            Func<T1, T2, T3, Task> handle) =>
            new AnonymousCommandHandler(
                async context => await handle(
                                     context.ParseResult.GetValueFor(symbol1)!,
                                     context.ParseResult.GetValueFor(symbol2)!,
                                     context.ParseResult.GetValueFor(symbol3)!));

        /// <summary>
        /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,Task}"/>.
        /// </summary>
        public static ICommandHandler Create<T1, T2, T3, T4>(
            IValueDescriptor<T1> symbol1,
            IValueDescriptor<T2> symbol2,
            IValueDescriptor<T3> symbol3,
            IValueDescriptor<T4> symbol4,
            Func<T1, T2, T3, T4, Task> handle) =>
            new AnonymousCommandHandler(
                async context => await handle(
                                     context.ParseResult.GetValueFor(symbol1)!,
                                     context.ParseResult.GetValueFor(symbol2)!,
                                     context.ParseResult.GetValueFor(symbol3)!,
                                     context.ParseResult.GetValueFor(symbol4)!));

        /// <summary>
        /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,Task}"/>.
        /// </summary>
        public static ICommandHandler Create<T1, T2, T3, T4, T5>(
            IValueDescriptor<T1> symbol1,
            IValueDescriptor<T2> symbol2,
            IValueDescriptor<T3> symbol3,
            IValueDescriptor<T4> symbol4,
            IValueDescriptor<T5> symbol5,
            Func<T1, T2, T3, T4, T5, Task> handle) =>
            new AnonymousCommandHandler(
                async context => await handle(
                                     context.ParseResult.GetValueFor(symbol1)!,
                                     context.ParseResult.GetValueFor(symbol2)!,
                                     context.ParseResult.GetValueFor(symbol3)!,
                                     context.ParseResult.GetValueFor(symbol4)!,
                                     context.ParseResult.GetValueFor(symbol5)!));

        /// <summary>
        /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,Task}"/>.
        /// </summary>
        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6>(
            IValueDescriptor<T1> symbol1,
            IValueDescriptor<T2> symbol2,
            IValueDescriptor<T3> symbol3,
            IValueDescriptor<T4> symbol4,
            IValueDescriptor<T5> symbol5,
            IValueDescriptor<T6> symbol6,
            Func<T1, T2, T3, T4, T5, T6, Task> handle) =>
            new AnonymousCommandHandler(
                async context => await handle(
                                     context.ParseResult.GetValueFor(symbol1)!,
                                     context.ParseResult.GetValueFor(symbol2)!,
                                     context.ParseResult.GetValueFor(symbol3)!,
                                     context.ParseResult.GetValueFor(symbol4)!,
                                     context.ParseResult.GetValueFor(symbol5)!,
                                     context.ParseResult.GetValueFor(symbol6)!));

        /// <summary>
        /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,Task}"/>.
        /// </summary>
        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7>(
            IValueDescriptor<T1> symbol1,
            IValueDescriptor<T2> symbol2,
            IValueDescriptor<T3> symbol3,
            IValueDescriptor<T4> symbol4,
            IValueDescriptor<T5> symbol5,
            IValueDescriptor<T6> symbol6,
            IValueDescriptor<T7> symbol7,
            Func<T1, T2, T3, T4, T5, T6, T7, Task> handle) =>
            new AnonymousCommandHandler(
                async context => await handle(
                                     context.ParseResult.GetValueFor(symbol1)!,
                                     context.ParseResult.GetValueFor(symbol2)!,
                                     context.ParseResult.GetValueFor(symbol3)!,
                                     context.ParseResult.GetValueFor(symbol4)!,
                                     context.ParseResult.GetValueFor(symbol5)!,
                                     context.ParseResult.GetValueFor(symbol6)!,
                                     context.ParseResult.GetValueFor(symbol7)!));

        /// <summary>
        /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,Task}"/>.
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
            Func<T1, T2, T3, T4, T5, T6, T7, T8, Task> handle) =>
            new AnonymousCommandHandler(
                async context => await handle(
                                     context.ParseResult.GetValueFor(symbol1)!,
                                     context.ParseResult.GetValueFor(symbol2)!,
                                     context.ParseResult.GetValueFor(symbol3)!,
                                     context.ParseResult.GetValueFor(symbol4)!,
                                     context.ParseResult.GetValueFor(symbol5)!,
                                     context.ParseResult.GetValueFor(symbol6)!,
                                     context.ParseResult.GetValueFor(symbol7)!,
                                     context.ParseResult.GetValueFor(symbol8)!));

        /// <summary>
        /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,Task}"/>.
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
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, Task> handle) =>
            new AnonymousCommandHandler(
                async context => await handle(
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
        /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,Task}"/>.
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
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, Task> handle) =>
            new AnonymousCommandHandler(
                async context => await handle(
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
        /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,Task}"/>.
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
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, Task> handle) =>
            new AnonymousCommandHandler(
                async context => await handle(
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
        /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,Task}"/>.
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
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, Task> handle) =>
            new AnonymousCommandHandler(
                async context => await handle(
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
        /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,Task}"/>.
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
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, Task> handle) =>
            new AnonymousCommandHandler(
                async context => await handle(
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
        /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,Task}"/>.
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
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, Task> handle) =>
            new AnonymousCommandHandler(
                async context => await handle(
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
        /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,Task}"/>.
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
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, Task> handle) =>
            new AnonymousCommandHandler(
                async context => await handle(
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
        /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,Task}"/>.
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
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, Task> handle) =>
            new AnonymousCommandHandler(
                async context => await handle(
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