
// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// WARINING: Generated code.

using System.CommandLine.Binding;
using System.Reflection;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    public static class CommandHandler
    {
        public static ICommandHandler Create(MethodInfo method) =>
            HandlerDescriptor.FromMethodInfo(method).GetCommandHandler();

        public static ICommandHandler Create(Action action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1>(
            Action<T1> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2>(
            Action<T1, T2> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3>(
            Action<T1, T2, T3> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4>(
            Action<T1, T2, T3, T4> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5>(
            Action<T1, T2, T3, T4, T5> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6>(
            Action<T1, T2, T3, T4, T5, T6> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7>(
            Action<T1, T2, T3, T4, T5, T6, T7> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8>(
            Action<T1, T2, T3, T4, T5, T6, T7, T8> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
            Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
            Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
            Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
            Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
            Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(
            Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();


        public static ICommandHandler Create(Func<int> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1>(
            Func<T1, int> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2>(
            Func<T1, T2, int> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3>(
            Func<T1, T2, T3, int> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4>(
            Func<T1, T2, T3, T4, int> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5>(
            Func<T1, T2, T3, T4, T5, int> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6>(
            Func<T1, T2, T3, T4, T5, T6, int> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7>(
            Func<T1, T2, T3, T4, T5, T6, T7, int> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8>(
            Func<T1, T2, T3, T4, T5, T6, T7, T8, int> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, int> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, int> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, int> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, int> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, int> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, int> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, int> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, int> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();


        public static ICommandHandler Create(Func<Task> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1>(
            Func<T1, Task> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2>(
            Func<T1, T2, Task> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3>(
            Func<T1, T2, T3, Task> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4>(
            Func<T1, T2, T3, T4, Task> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5>(
            Func<T1, T2, T3, T4, T5, Task> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6>(
            Func<T1, T2, T3, T4, T5, T6, Task> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7>(
            Func<T1, T2, T3, T4, T5, T6, T7, Task> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8>(
            Func<T1, T2, T3, T4, T5, T6, T7, T8, Task> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, Task> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, Task> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, Task> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, Task> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, Task> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, Task> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, Task> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, Task> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();



        public static ICommandHandler Create(Func<Task<int>> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1>(
            Func<T1, Task<int>> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2>(
            Func<T1, T2, Task<int>> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3>(
            Func<T1, T2, T3, Task<int>> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4>(
            Func<T1, T2, T3, T4, Task<int>> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5>(
            Func<T1, T2, T3, T4, T5, Task<int>> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6>(
            Func<T1, T2, T3, T4, T5, T6, Task<int>> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7>(
            Func<T1, T2, T3, T4, T5, T6, T7, Task<int>> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8>(
            Func<T1, T2, T3, T4, T5, T6, T7, T8, Task<int>> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, Task<int>> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, Task<int>> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, Task<int>> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, Task<int>> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, Task<int>> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, Task<int>> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, Task<int>> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, Task<int>> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();


        internal static async Task<int> GetResultCodeAsync(object value, InvocationContext context)
        {
            switch (value)
            {
                case Task<int> resultCodeTask:
                    return await resultCodeTask;
                case Task task:
                    await task;
                    return context.ResultCode;
                case int resultCode:
                    return resultCode;
                case null:
                    return context.ResultCode;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
