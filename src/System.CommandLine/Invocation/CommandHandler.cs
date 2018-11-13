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
            object target = null) => 
            new MethodBindingCommandHandler(method, target);

        public static ICommandHandler Create(Action action) => 
            new MethodBindingCommandHandler(action);

        public static ICommandHandler Create<T>(
            Action<T> action) => 
            new MethodBindingCommandHandler(action);

        public static ICommandHandler Create<T>(
            Func<T, Task> action) =>
            new MethodBindingCommandHandler(action);

        public static ICommandHandler Create<T1, T2>(
            Action<T1, T2> action) => 
            new MethodBindingCommandHandler(action);

        public static ICommandHandler Create<T1, T2, T3>(
            Action<T1, T2, T3> action) => 
            new MethodBindingCommandHandler(action);

        public static ICommandHandler Create<T1, T2, T3, T4>(
            Action<T1, T2, T3, T4> action) => 
            new MethodBindingCommandHandler(action);

        public static ICommandHandler Create<T1, T2, T3, T4, T5>(
            Action<T1, T2, T3, T4, T5> action) => 
            new MethodBindingCommandHandler(action);

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6>(
            Action<T1, T2, T3, T4, T5, T6> action) => 
            new MethodBindingCommandHandler(action);

        public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7>(
            Action<T1, T2, T3, T4, T5, T6, T7> action) => 
            new MethodBindingCommandHandler(action);

        internal static async Task<int> GetResultCodeAsync(object value)
        {
            switch (value)
            {
                case Task<int> resultCodeTask:
                    return await resultCodeTask;
                case Task task:
                    await task;
                    return 0;
                case int resultCode:
                    return resultCode;
                case null:
                    return 0;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
