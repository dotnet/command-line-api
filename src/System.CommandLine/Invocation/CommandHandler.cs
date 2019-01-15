// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    public static class CommandHandler
    {
        public static ICommandHandler Create(
            MethodInfo method,
            Func<object> target = null) =>
            new MethodBindingCommandHandler(method, target);

        public static ICommandHandler Create(Action action) =>
            ReflectionCommandHandler.Create(action.Method, action.Target);

        public static ICommandHandler Create<T>(
            Action<T> action) =>
            ReflectionCommandHandler.Create(action.Method, action.Target);

        public static ICommandHandler Create<T1, T2>(
            Action<T1, T2> action) =>
            ReflectionCommandHandler.Create(action.Method, action.Target);

        public static ICommandHandler Create<T1, T2, T3>(
            Action<T1, T2, T3> action) =>
            ReflectionCommandHandler.Create(action.Method, action.Target);

        public static ICommandHandler Create<T1, T2, T3, T4>(
            Action<T1, T2, T3, T4> action) =>
            ReflectionCommandHandler.Create(action.Method, action.Target);

        public static ICommandHandler Create<T1, T2, T3, T4, T5>(
            Action<T1, T2, T3, T4, T5> action) =>
            ReflectionCommandHandler.Create(action.Method, action.Target);

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6>(
            Action<T1, T2, T3, T4, T5, T6> action) =>
            ReflectionCommandHandler.Create(action.Method, action.Target);

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7>(
            Action<T1, T2, T3, T4, T5, T6, T7> action) =>
            ReflectionCommandHandler.Create(action.Method, action.Target);

        public static ICommandHandler Create(Func<int> action) =>
            ReflectionCommandHandler.Create(action.Method, action.Target);

        public static ICommandHandler Create<T>(
            Func<T, int> action) =>
            ReflectionCommandHandler.Create(action.Method, action.Target);

        public static ICommandHandler Create<T1, T2>(
            Func<T1, T2, int> action) =>
            ReflectionCommandHandler.Create(action.Method, action.Target);

        public static ICommandHandler Create<T1, T2, T3>(
            Func<T1, T2, T3, int> action) =>
            ReflectionCommandHandler.Create(action.Method, action.Target);

        public static ICommandHandler Create<T1, T2, T3, T4>(
            Func<T1, T2, T3, T4, int> action) =>
            ReflectionCommandHandler.Create(action.Method, action.Target);

        public static ICommandHandler Create<T1, T2, T3, T4, T5>(
            Func<T1, T2, T3, T4, T5, int> action) =>
            ReflectionCommandHandler.Create(action.Method, action.Target);

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6>(
            Func<T1, T2, T3, T4, T5, T6, int> action) =>
            ReflectionCommandHandler.Create(action.Method, action.Target);

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7>(
            Func<T1, T2, T3, T4, T5, T6, T7, int> action) =>
            ReflectionCommandHandler.Create(action.Method, action.Target);

        public static ICommandHandler Create(Func<Task> action) =>
            ReflectionCommandHandler.Create(action.Method, action.Target);

        public static ICommandHandler Create<T>(
            Func<T, Task> action) =>
            ReflectionCommandHandler.Create(action.Method, action.Target);

        public static ICommandHandler Create<T1, T2>(
            Func<T1, T2, Task> action) =>
            ReflectionCommandHandler.Create(action.Method, action.Target);

        public static ICommandHandler Create<T1, T2, T3>(
            Func<T1, T2, T3, Task> action) =>
            ReflectionCommandHandler.Create(action.Method, action.Target);

        public static ICommandHandler Create<T1, T2, T3, T4>(
            Func<T1, T2, T3, T4, Task> action) =>
            ReflectionCommandHandler.Create(action.Method, action.Target);

        public static ICommandHandler Create<T1, T2, T3, T4, T5>(
            Func<T1, T2, T3, T4, T5, Task> action) =>
            ReflectionCommandHandler.Create(action.Method, action.Target);

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6>(
            Func<T1, T2, T3, T4, T5, T6, Task> action) =>
            ReflectionCommandHandler.Create(action.Method, action.Target);

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7>(
            Func<T1, T2, T3, T4, T5, T6, T7, Task> action) =>
            ReflectionCommandHandler.Create(action.Method, action.Target);

        public static ICommandHandler Create(Func<Task<int>> action) =>
            ReflectionCommandHandler.Create(action.Method, action.Target);

        public static ICommandHandler Create<T>(
            Func<T, Task<int>> action) =>
            ReflectionCommandHandler.Create(action.Method, action.Target);

        public static ICommandHandler Create<T1, T2>(
            Func<T1, T2, Task<int>> action) =>
            ReflectionCommandHandler.Create(action.Method, action.Target);

        public static ICommandHandler Create<T1, T2, T3>(
            Func<T1, T2, T3, Task<int>> action) =>
            ReflectionCommandHandler.Create(action.Method, action.Target);

        public static ICommandHandler Create<T1, T2, T3, T4>(
            Func<T1, T2, T3, T4, Task<int>> action) =>
            ReflectionCommandHandler.Create(action.Method, action.Target);

        public static ICommandHandler Create<T1, T2, T3, T4, T5>(
            Func<T1, T2, T3, T4, T5, Task<int>> action) =>
            ReflectionCommandHandler.Create(action.Method, action.Target);

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6>(
            Func<T1, T2, T3, T4, T5, T6, Task<int>> action) =>
            ReflectionCommandHandler.Create(action.Method, action.Target);

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7>(
            Func<T1, T2, T3, T4, T5, T6, T7, Task<int>> action) =>
            ReflectionCommandHandler.Create(action.Method, action.Target);

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
