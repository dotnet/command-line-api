﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.Reflection;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    public static class CommandHandler
    {
        public static ICommandHandler Create(Delegate @delegate) =>
            HandlerDescriptor.FromDelegate(@delegate).GetCommandHandler();

        public static ICommandHandler Create(MethodInfo method, object? target = null) =>
            HandlerDescriptor.FromMethodInfo(method, target).GetCommandHandler();

        public static ICommandHandler Create(Action action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T>(
            Action<T> action) =>
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

        public static ICommandHandler Create(Func<int> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T>(
            Func<T, int> action) =>
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

        public static ICommandHandler Create(Func<Task> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T>(
            Func<T, Task> action) =>
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

        public static ICommandHandler Create(Func<Task<int>> action) =>
            HandlerDescriptor.FromDelegate(action).GetCommandHandler();

        public static ICommandHandler Create<T>(
            Func<T, Task<int>> action) =>
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
