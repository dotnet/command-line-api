// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    public static partial class CommandHandler
    {
        public static ICommandHandler Create(Action action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1>(
            IValueDescriptor<T1> symbol1,
            Func<T1, Task> handle) =>
            new AnonymousCommandHandler(
                async context => await handle(
                                     context.ParseResult.ValueFor(symbol1)!));

        public static ICommandHandler Create<T1, T2>(
            IValueDescriptor<T1> symbol1,
            IValueDescriptor<T2> symbol2,
            Func<T1, T2, Task> handle) =>
            new AnonymousCommandHandler(
                async context => await handle(
                                     context.ParseResult.ValueFor(symbol1)!,
                                     context.ParseResult.ValueFor(symbol2)!));

        public static ICommandHandler Create<T1, T2, T3>(
            IValueDescriptor<T1> symbol1,
            IValueDescriptor<T2> symbol2,
            IValueDescriptor<T3> symbol3,
            Func<T1, T2, T3, Task> handle) =>
            new AnonymousCommandHandler(
                async context => await handle(
                                     context.ParseResult.ValueFor(symbol1)!,
                                     context.ParseResult.ValueFor(symbol2)!,
                                     context.ParseResult.ValueFor(symbol3)!));

        public static ICommandHandler Create<T1, T2, T3, T4>(
            IValueDescriptor<T1> symbol1,
            IValueDescriptor<T2> symbol2,
            IValueDescriptor<T3> symbol3,
            IValueDescriptor<T4> symbol4,
            Func<T1, T2, T3, T4, Task> handle) =>
            new AnonymousCommandHandler(
                async context => await handle(
                                     context.ParseResult.ValueFor(symbol1)!,
                                     context.ParseResult.ValueFor(symbol2)!,
                                     context.ParseResult.ValueFor(symbol3)!,
                                     context.ParseResult.ValueFor(symbol4)!));

        public static ICommandHandler Create<T1, T2, T3, T4, T5>(
            IValueDescriptor<T1> symbol1,
            IValueDescriptor<T2> symbol2,
            IValueDescriptor<T3> symbol3,
            IValueDescriptor<T4> symbol4,
            IValueDescriptor<T5> symbol5,
            Func<T1, T2, T3, T4, T5, Task> handle) =>
            new AnonymousCommandHandler(
                async context => await handle(
                                     context.ParseResult.ValueFor(symbol1)!,
                                     context.ParseResult.ValueFor(symbol2)!,
                                     context.ParseResult.ValueFor(symbol3)!,
                                     context.ParseResult.ValueFor(symbol4)!,
                                     context.ParseResult.ValueFor(symbol5)!));

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
                                     context.ParseResult.ValueFor(symbol1)!,
                                     context.ParseResult.ValueFor(symbol2)!,
                                     context.ParseResult.ValueFor(symbol3)!,
                                     context.ParseResult.ValueFor(symbol4)!,
                                     context.ParseResult.ValueFor(symbol5)!,
                                     context.ParseResult.ValueFor(symbol6)!));

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
                                     context.ParseResult.ValueFor(symbol1)!,
                                     context.ParseResult.ValueFor(symbol2)!,
                                     context.ParseResult.ValueFor(symbol3)!,
                                     context.ParseResult.ValueFor(symbol4)!,
                                     context.ParseResult.ValueFor(symbol5)!,
                                     context.ParseResult.ValueFor(symbol6)!,
                                     context.ParseResult.ValueFor(symbol7)!));

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
                                     context.ParseResult.ValueFor(symbol1)!,
                                     context.ParseResult.ValueFor(symbol2)!,
                                     context.ParseResult.ValueFor(symbol3)!,
                                     context.ParseResult.ValueFor(symbol4)!,
                                     context.ParseResult.ValueFor(symbol5)!,
                                     context.ParseResult.ValueFor(symbol6)!,
                                     context.ParseResult.ValueFor(symbol7)!,
                                     context.ParseResult.ValueFor(symbol8)!));

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
                                     context.ParseResult.ValueFor(symbol1)!,
                                     context.ParseResult.ValueFor(symbol2)!,
                                     context.ParseResult.ValueFor(symbol3)!,
                                     context.ParseResult.ValueFor(symbol4)!,
                                     context.ParseResult.ValueFor(symbol5)!,
                                     context.ParseResult.ValueFor(symbol6)!,
                                     context.ParseResult.ValueFor(symbol7)!,
                                     context.ParseResult.ValueFor(symbol8)!,
                                     context.ParseResult.ValueFor(symbol9)!));

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
                                     context.ParseResult.ValueFor(symbol1)!,
                                     context.ParseResult.ValueFor(symbol2)!,
                                     context.ParseResult.ValueFor(symbol3)!,
                                     context.ParseResult.ValueFor(symbol4)!,
                                     context.ParseResult.ValueFor(symbol5)!,
                                     context.ParseResult.ValueFor(symbol6)!,
                                     context.ParseResult.ValueFor(symbol7)!,
                                     context.ParseResult.ValueFor(symbol8)!,
                                     context.ParseResult.ValueFor(symbol9)!,
                                     context.ParseResult.ValueFor(symbol10)!));

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
                                     context.ParseResult.ValueFor(symbol1)!,
                                     context.ParseResult.ValueFor(symbol2)!,
                                     context.ParseResult.ValueFor(symbol3)!,
                                     context.ParseResult.ValueFor(symbol4)!,
                                     context.ParseResult.ValueFor(symbol5)!,
                                     context.ParseResult.ValueFor(symbol6)!,
                                     context.ParseResult.ValueFor(symbol7)!,
                                     context.ParseResult.ValueFor(symbol8)!,
                                     context.ParseResult.ValueFor(symbol9)!,
                                     context.ParseResult.ValueFor(symbol10)!,
                                     context.ParseResult.ValueFor(symbol11)!));

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
                                     context.ParseResult.ValueFor(symbol1)!,
                                     context.ParseResult.ValueFor(symbol2)!,
                                     context.ParseResult.ValueFor(symbol3)!,
                                     context.ParseResult.ValueFor(symbol4)!,
                                     context.ParseResult.ValueFor(symbol5)!,
                                     context.ParseResult.ValueFor(symbol6)!,
                                     context.ParseResult.ValueFor(symbol7)!,
                                     context.ParseResult.ValueFor(symbol8)!,
                                     context.ParseResult.ValueFor(symbol9)!,
                                     context.ParseResult.ValueFor(symbol10)!,
                                     context.ParseResult.ValueFor(symbol11)!,
                                     context.ParseResult.ValueFor(symbol12)!));

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
                                     context.ParseResult.ValueFor(symbol1)!,
                                     context.ParseResult.ValueFor(symbol2)!,
                                     context.ParseResult.ValueFor(symbol3)!,
                                     context.ParseResult.ValueFor(symbol4)!,
                                     context.ParseResult.ValueFor(symbol5)!,
                                     context.ParseResult.ValueFor(symbol6)!,
                                     context.ParseResult.ValueFor(symbol7)!,
                                     context.ParseResult.ValueFor(symbol8)!,
                                     context.ParseResult.ValueFor(symbol9)!,
                                     context.ParseResult.ValueFor(symbol10)!,
                                     context.ParseResult.ValueFor(symbol11)!,
                                     context.ParseResult.ValueFor(symbol12)!,
                                     context.ParseResult.ValueFor(symbol13)!));

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
                                     context.ParseResult.ValueFor(symbol1)!,
                                     context.ParseResult.ValueFor(symbol2)!,
                                     context.ParseResult.ValueFor(symbol3)!,
                                     context.ParseResult.ValueFor(symbol4)!,
                                     context.ParseResult.ValueFor(symbol5)!,
                                     context.ParseResult.ValueFor(symbol6)!,
                                     context.ParseResult.ValueFor(symbol7)!,
                                     context.ParseResult.ValueFor(symbol8)!,
                                     context.ParseResult.ValueFor(symbol9)!,
                                     context.ParseResult.ValueFor(symbol10)!,
                                     context.ParseResult.ValueFor(symbol11)!,
                                     context.ParseResult.ValueFor(symbol12)!,
                                     context.ParseResult.ValueFor(symbol13)!,
                                     context.ParseResult.ValueFor(symbol14)!));

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
                                     context.ParseResult.ValueFor(symbol1)!,
                                     context.ParseResult.ValueFor(symbol2)!,
                                     context.ParseResult.ValueFor(symbol3)!,
                                     context.ParseResult.ValueFor(symbol4)!,
                                     context.ParseResult.ValueFor(symbol5)!,
                                     context.ParseResult.ValueFor(symbol6)!,
                                     context.ParseResult.ValueFor(symbol7)!,
                                     context.ParseResult.ValueFor(symbol8)!,
                                     context.ParseResult.ValueFor(symbol9)!,
                                     context.ParseResult.ValueFor(symbol10)!,
                                     context.ParseResult.ValueFor(symbol11)!,
                                     context.ParseResult.ValueFor(symbol12)!,
                                     context.ParseResult.ValueFor(symbol13)!,
                                     context.ParseResult.ValueFor(symbol14)!,
                                     context.ParseResult.ValueFor(symbol15)!));

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
                                     context.ParseResult.ValueFor(symbol1)!,
                                     context.ParseResult.ValueFor(symbol2)!,
                                     context.ParseResult.ValueFor(symbol3)!,
                                     context.ParseResult.ValueFor(symbol4)!,
                                     context.ParseResult.ValueFor(symbol5)!,
                                     context.ParseResult.ValueFor(symbol6)!,
                                     context.ParseResult.ValueFor(symbol7)!,
                                     context.ParseResult.ValueFor(symbol8)!,
                                     context.ParseResult.ValueFor(symbol9)!,
                                     context.ParseResult.ValueFor(symbol10)!,
                                     context.ParseResult.ValueFor(symbol11)!,
                                     context.ParseResult.ValueFor(symbol12)!,
                                     context.ParseResult.ValueFor(symbol13)!,
                                     context.ParseResult.ValueFor(symbol14)!,
                                     context.ParseResult.ValueFor(symbol15)!,
                                     context.ParseResult.ValueFor(symbol16)!));

        private class AnonymousCommandHandler : ICommandHandler
        {
            private readonly Func<InvocationContext, Task> _getResult;

            public AnonymousCommandHandler(Func<InvocationContext, Task> getResult)
            {
                _getResult = getResult;
            }

            public Task<int> InvokeAsync(InvocationContext context) =>
                GetExitCodeAsync(_getResult(context), context);
        }

        internal static async Task<int> GetExitCodeAsync(object value, InvocationContext context)
        {
            switch (value)
            {
                case Task<int> exitCodeTask:
                    return await exitCodeTask;
                case Task task:
                    await task;
                    return context.ExitCode;
                case int exitCode:
                    return exitCode;
                case null:
                    return context.ExitCode;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}