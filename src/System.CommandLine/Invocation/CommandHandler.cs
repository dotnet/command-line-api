// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.Reflection;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    public static class CommandHandler
    {
        public static ICommandHandler Create(MethodInfo method) =>
            new MethodInfoHandlerDescriptor(method).GetCommandHandler();

        public static ICommandHandler Create(Action action) =>
            new DelegateHandlerDescriptor(action).GetCommandHandler();

        public static ICommandHandler Create<T>(
            Action<T> action) =>
            new DelegateHandlerDescriptor(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2>(
            Action<T1, T2> action) =>
            new DelegateHandlerDescriptor(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3>(
            Action<T1, T2, T3> action) =>
            new DelegateHandlerDescriptor(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4>(
            Action<T1, T2, T3, T4> action) =>
            new DelegateHandlerDescriptor(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5>(
            Action<T1, T2, T3, T4, T5> action) =>
            new DelegateHandlerDescriptor(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6>(
            Action<T1, T2, T3, T4, T5, T6> action) =>
            new DelegateHandlerDescriptor(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7>(
            Action<T1, T2, T3, T4, T5, T6, T7> action) =>
            new DelegateHandlerDescriptor(action).GetCommandHandler();

        public static ICommandHandler Create(Func<int> action) =>
            new DelegateHandlerDescriptor(action).GetCommandHandler();

        public static ICommandHandler Create<T>(
            Func<T, int> action) =>
            new DelegateHandlerDescriptor(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2>(
            Func<T1, T2, int> action) =>
            new DelegateHandlerDescriptor(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3>(
            Func<T1, T2, T3, int> action) =>
            new DelegateHandlerDescriptor(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4>(
            Func<T1, T2, T3, T4, int> action) =>
            new DelegateHandlerDescriptor(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5>(
            Func<T1, T2, T3, T4, T5, int> action) =>
            new DelegateHandlerDescriptor(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6>(
            Func<T1, T2, T3, T4, T5, T6, int> action) =>
            new DelegateHandlerDescriptor(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7>(
            Func<T1, T2, T3, T4, T5, T6, T7, int> action) =>
            new DelegateHandlerDescriptor(action).GetCommandHandler();

        public static ICommandHandler Create(Func<Task> action) =>
            new DelegateHandlerDescriptor(action).GetCommandHandler();

        public static ICommandHandler Create<T>(
            Func<T, Task> action) =>
            new DelegateHandlerDescriptor(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2>(
            Func<T1, T2, Task> action) =>
            new DelegateHandlerDescriptor(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3>(
            Func<T1, T2, T3, Task> action) =>
            new DelegateHandlerDescriptor(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4>(
            Func<T1, T2, T3, T4, Task> action) =>
            new DelegateHandlerDescriptor(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5>(
            Func<T1, T2, T3, T4, T5, Task> action) =>
            new DelegateHandlerDescriptor(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6>(
            Func<T1, T2, T3, T4, T5, T6, Task> action) =>
            new DelegateHandlerDescriptor(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7>(
            Func<T1, T2, T3, T4, T5, T6, T7, Task> action) =>
            new DelegateHandlerDescriptor(action).GetCommandHandler();

        public static ICommandHandler Create(Func<Task<int>> action) =>
            new DelegateHandlerDescriptor(action).GetCommandHandler();

        public static ICommandHandler Create<T>(
            Func<T, Task<int>> action) =>
            new DelegateHandlerDescriptor(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2>(
            Func<T1, T2, Task<int>> action) =>
            new DelegateHandlerDescriptor(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3>(
            Func<T1, T2, T3, Task<int>> action) =>
            new DelegateHandlerDescriptor(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4>(
            Func<T1, T2, T3, T4, Task<int>> action) =>
            new DelegateHandlerDescriptor(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5>(
            Func<T1, T2, T3, T4, T5, Task<int>> action) =>
            new DelegateHandlerDescriptor(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6>(
            Func<T1, T2, T3, T4, T5, T6, Task<int>> action) =>
            new DelegateHandlerDescriptor(action).GetCommandHandler();

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7>(
            Func<T1, T2, T3, T4, T5, T6, T7, Task<int>> action) =>
            new DelegateHandlerDescriptor(action).GetCommandHandler();

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
