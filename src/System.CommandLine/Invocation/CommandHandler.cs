// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    public static class CommandHandler
    {
        public static ReflectionCommandHandler Create<T>()
        {
            return CreateHandler(typeof(T), null, null);
        }

        public static ReflectionCommandHandler Create<T>(string methodName)
        {
            var type = typeof(T);
            var methodInfo = type.GetMethod(methodName);
            return CreateHandler(type, methodInfo, null);
        }

        public static ReflectionCommandHandler Create(Action action) =>
            CreateHandler(action.Method, action.Target);

        private static ReflectionCommandHandler CreateHandler(
            Type type,
            MethodInfo method,
            object target)
        {
            return new ReflectionCommandHandler(type, method, target);
        }

        private static ReflectionCommandHandler CreateHandler(
            MethodInfo method,
            object target)
        {
            return CreateHandler(method.DeclaringType, method, target);
        }

        public static ReflectionCommandHandler Create<T>(
            Action<T> action) =>
            CreateHandler(null, action.Method, action.Target);

        public static ReflectionCommandHandler Create<T1, T2>(
            Action<T1, T2> action) =>
            CreateHandler(null, action.Method, action.Target);

        public static ReflectionCommandHandler Create<T1, T2, T3>(
            Action<T1, T2, T3> action) =>
            CreateHandler(null, action.Method, action.Target);

        public static ReflectionCommandHandler Create<T1, T2, T3, T4>(
            Action<T1, T2, T3, T4> action) =>
            CreateHandler(action.Method, action.Target);

        public static ReflectionCommandHandler Create<T1, T2, T3, T4, T5>(
            Action<T1, T2, T3, T4, T5> action) =>
            CreateHandler(action.Method, action.Target);

        public static ReflectionCommandHandler Create<T1, T2, T3, T4, T5, T6>(
            Action<T1, T2, T3, T4, T5, T6> action) =>
            CreateHandler(action.Method, action.Target);

        public static ReflectionCommandHandler Create<T1, T2, T3, T4, T5, T6, T7>(
            Action<T1, T2, T3, T4, T5, T6, T7> action) =>
            CreateHandler(action.Method, action.Target);

        public static ReflectionCommandHandler Create(Func<int> action, Command command = null) =>
            CreateHandler(action.Method, action.Target);

        public static ReflectionCommandHandler Create<T>(
            Func<T, int> action) =>
            CreateHandler(action.Method, action.Target);

        public static ReflectionCommandHandler Create<T1, T2>(
            Func<T1, T2, int> action) =>
            CreateHandler(action.Method, action.Target);

        public static ReflectionCommandHandler Create<T1, T2, T3>(
            Func<T1, T2, T3, int> action) =>
            CreateHandler(action.Method, action.Target);

        public static ReflectionCommandHandler Create<T1, T2, T3, T4>(
            Func<T1, T2, T3, T4, int> action) =>
            CreateHandler(action.Method, action.Target);

        public static ReflectionCommandHandler Create<T1, T2, T3, T4, T5>(
            Func<T1, T2, T3, T4, T5, int> action) =>
            CreateHandler(action.Method, action.Target);

        public static ReflectionCommandHandler Create<T1, T2, T3, T4, T5, T6>(
            Func<T1, T2, T3, T4, T5, T6, int> action) =>
            CreateHandler(action.Method, action.Target);

        public static ReflectionCommandHandler Create<T1, T2, T3, T4, T5, T6, T7>(
            Func<T1, T2, T3, T4, T5, T6, T7, int> action) =>
            CreateHandler(action.Method, action.Target);

        public static ReflectionCommandHandler Create(Func<Task> action) =>
            CreateHandler(action.Method, action.Target);

        public static ReflectionCommandHandler Create<T>(
            Func<T, Task> action) =>
            CreateHandler(action.Method, action.Target);

        public static ReflectionCommandHandler Create<T1, T2>(
            Func<T1, T2, Task> action) =>
            CreateHandler(action.Method, action.Target);

        public static ReflectionCommandHandler Create<T1, T2, T3>(
            Func<T1, T2, T3, Task> action) =>
            CreateHandler(action.Method, action.Target);

        public static ReflectionCommandHandler Create<T1, T2, T3, T4>(
            Func<T1, T2, T3, T4, Task> action) =>
            CreateHandler(action.Method, action.Target);

        public static ReflectionCommandHandler Create<T1, T2, T3, T4, T5>(
            Func<T1, T2, T3, T4, T5, Task> action) =>
            CreateHandler(action.Method, action.Target);

        public static ReflectionCommandHandler Create<T1, T2, T3, T4, T5, T6>(
            Func<T1, T2, T3, T4, T5, T6, Task> action) =>
            CreateHandler(action.Method, action.Target);

        public static ReflectionCommandHandler Create<T1, T2, T3, T4, T5, T6, T7>(
            Func<T1, T2, T3, T4, T5, T6, T7, Task> action) =>
            CreateHandler(action.Method, action.Target);

        public static ReflectionCommandHandler Create(Func<Task<int>> action) =>
            CreateHandler(action.Method, action.Target);

        public static ReflectionCommandHandler Create<T>(
            Func<T, Task<int>> action) =>
            CreateHandler(action.Method, action.Target);

        public static ReflectionCommandHandler Create<T1, T2>(
            Func<T1, T2, Task<int>> action) =>
            CreateHandler(action.Method, action.Target);

        public static ReflectionCommandHandler Create<T1, T2, T3>(
            Func<T1, T2, T3, Task<int>> action) =>
            CreateHandler(action.Method, action.Target);

        public static ReflectionCommandHandler Create<T1, T2, T3, T4>(
            Func<T1, T2, T3, T4, Task<int>> action) =>
            CreateHandler(action.Method, action.Target);

        public static ReflectionCommandHandler Create<T1, T2, T3, T4, T5>(
            Func<T1, T2, T3, T4, T5, Task<int>> action) =>
            CreateHandler(action.Method, action.Target);

        public static ReflectionCommandHandler Create<T1, T2, T3, T4, T5, T6>(
            Func<T1, T2, T3, T4, T5, T6, Task<int>> action) =>
            CreateHandler(action.Method, action.Target);

        public static ReflectionCommandHandler Create<T1, T2, T3, T4, T5, T6, T7>(
            Func<T1, T2, T3, T4, T5, T6, T7, Task<int>> action) =>
            CreateHandler(action.Method, action.Target);

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
