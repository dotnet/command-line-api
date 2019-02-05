// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.Reflection;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    public static class CommandHandler
    {
        public static ReflectionCommandHandler Create(
            MethodInfo method,
            Func<object> target = null, Command command = null) =>
            CreateHandler(command, method, target);

        public static ReflectionCommandHandler Create<T>(Command command = null)
        {
            return CreateHandler(command, typeof(T), null, null);
        }

        public static ReflectionCommandHandler Create<T>(string methodName, Command command = null)
        {
            var type = typeof(T);
            var methodInfo = type.GetMethod(methodName);
            return CreateHandler(command,type, methodInfo, null);
        }

        public static ReflectionCommandHandler Create(Action action, Command command = null) =>
            CreateHandler(command, action.Method, action.Target);

        private static ReflectionCommandHandler CreateHandler(
            Command command, Type type, MethodInfo method, object target)
        {
            var handler = ReflectionCommandHandler.Create(type, method, target);
            handler.Binder.AddBindings(type, method, command);
            return handler;
        }

        private static ReflectionCommandHandler CreateHandler(
         Command command,  MethodInfo method, object target)
        {
            return CreateHandler(command, method.DeclaringType, method, target);
        }

        public static ReflectionCommandHandler Create<T>(
            Action<T> action, Command command = null) =>
            CreateHandler(command,  action.Method, action.Target);

        public static ReflectionCommandHandler Create<T1, T2>(
            Action<T1, T2> action, Command command = null) =>
            CreateHandler(command, action.Method, action.Target);

        public static ReflectionCommandHandler Create<T1, T2, T3>(
            Action<T1, T2, T3> action, Command command = null) =>
            CreateHandler(command, action.Method, action.Target);

        public static ReflectionCommandHandler Create<T1, T2, T3, T4>(
            Action<T1, T2, T3, T4> action, Command command = null) =>
            CreateHandler(command, action.Method, action.Target);

        public static ReflectionCommandHandler Create<T1, T2, T3, T4, T5>(
            Action<T1, T2, T3, T4, T5> action, Command command = null) =>
            CreateHandler(command, action.Method, action.Target);

        public static ReflectionCommandHandler Create<T1, T2, T3, T4, T5, T6>(
            Action<T1, T2, T3, T4, T5, T6> action, Command command = null) =>
            CreateHandler(command, action.Method, action.Target);

        public static ReflectionCommandHandler Create<T1, T2, T3, T4, T5, T6, T7>(
            Action<T1, T2, T3, T4, T5, T6, T7> action, Command command = null) =>
            CreateHandler(command, action.Method, action.Target);

        public static ReflectionCommandHandler Create(Func<int> action, Command command = null) =>
            CreateHandler(command, action.Method, action.Target);

        public static ReflectionCommandHandler Create<T>(
            Func<T, int> action, Command command = null) =>
            CreateHandler(command, action.Method, action.Target);

        public static ReflectionCommandHandler Create<T1, T2>(
            Func<T1, T2, int> action, Command command = null) =>
            CreateHandler(command, action.Method, action.Target);

        public static ReflectionCommandHandler Create<T1, T2, T3>(
            Func<T1, T2, T3, int> action, Command command = null) =>
            CreateHandler(command, action.Method, action.Target);

        public static ReflectionCommandHandler Create<T1, T2, T3, T4>(
            Func<T1, T2, T3, T4, int> action, Command command = null) =>
            CreateHandler(command, action.Method, action.Target);

        public static ReflectionCommandHandler Create<T1, T2, T3, T4, T5>(
            Func<T1, T2, T3, T4, T5, int> action, Command command = null) =>
            CreateHandler(command, action.Method, action.Target);

        public static ReflectionCommandHandler Create<T1, T2, T3, T4, T5, T6>(
            Func<T1, T2, T3, T4, T5, T6, int> action, Command command = null) =>
            CreateHandler(command, action.Method, action.Target);

        public static ReflectionCommandHandler Create<T1, T2, T3, T4, T5, T6, T7>(
            Func<T1, T2, T3, T4, T5, T6, T7, int> action, Command command = null) =>
            CreateHandler(command, action.Method, action.Target);

        public static ReflectionCommandHandler Create(Func<Task> action, Command command = null) =>
            CreateHandler(command, action.Method, action.Target);

        public static ReflectionCommandHandler Create<T>(
            Func<T, Task> action, Command command = null) =>
            CreateHandler(command, action.Method, action.Target);

        public static ReflectionCommandHandler Create<T1, T2>(
            Func<T1, T2, Task> action, Command command = null) =>
            CreateHandler(command, action.Method, action.Target);

        public static ReflectionCommandHandler Create<T1, T2, T3>(
            Func<T1, T2, T3, Task> action, Command command = null) =>
            CreateHandler(command, action.Method, action.Target);

        public static ReflectionCommandHandler Create<T1, T2, T3, T4>(
            Func<T1, T2, T3, T4, Task> action, Command command = null) =>
            CreateHandler(command, action.Method, action.Target);

        public static ReflectionCommandHandler Create<T1, T2, T3, T4, T5>(
            Func<T1, T2, T3, T4, T5, Task> action, Command command = null) =>
            CreateHandler(command, action.Method, action.Target);

        public static ReflectionCommandHandler Create<T1, T2, T3, T4, T5, T6>(
            Func<T1, T2, T3, T4, T5, T6, Task> action, Command command = null) =>
            CreateHandler(command, action.Method, action.Target);

        public static ReflectionCommandHandler Create<T1, T2, T3, T4, T5, T6, T7>(
            Func<T1, T2, T3, T4, T5, T6, T7, Task> action, Command command = null) =>
            CreateHandler(command, action.Method, action.Target);

        public static ReflectionCommandHandler Create(Func<Task<int>> action, Command command = null) =>
            CreateHandler(command, action.Method, action.Target);

        public static ReflectionCommandHandler Create<T>(
            Func<T, Task<int>> action, Command command = null) =>
            CreateHandler(command, action.Method, action.Target);

        public static ReflectionCommandHandler Create<T1, T2>(
            Func<T1, T2, Task<int>> action, Command command = null) =>
            CreateHandler(command, action.Method, action.Target);

        public static ReflectionCommandHandler Create<T1, T2, T3>(
            Func<T1, T2, T3, Task<int>> action, Command command = null) =>
            CreateHandler(command, action.Method, action.Target);

        public static ReflectionCommandHandler Create<T1, T2, T3, T4>(
            Func<T1, T2, T3, T4, Task<int>> action, Command command = null) =>
            CreateHandler(command, action.Method, action.Target);

        public static ReflectionCommandHandler Create<T1, T2, T3, T4, T5>(
            Func<T1, T2, T3, T4, T5, Task<int>> action, Command command = null) =>
            CreateHandler(command, action.Method, action.Target);

        public static ReflectionCommandHandler Create<T1, T2, T3, T4, T5, T6>(
            Func<T1, T2, T3, T4, T5, T6, Task<int>> action, Command command = null) =>
            CreateHandler(command, action.Method, action.Target);

        public static ReflectionCommandHandler Create<T1, T2, T3, T4, T5, T6, T7>(
            Func<T1, T2, T3, T4, T5, T6, T7, Task<int>> action, Command command = null) =>
            CreateHandler(command, action.Method, action.Target);

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
