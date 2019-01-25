// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    public class ReflectionCommandHandler : IBoundCommandHandler
    {
        private ReflectionCommandHandler(Type targetType)
        {
            TargetType = targetType;
            Binder = new ReflectionBinder(TargetType);
        }

        public static ReflectionCommandHandler Create(MethodInfo methodInfo)
        {
            var handler = Create(methodInfo.DeclaringType, methodInfo, null);
            return handler;
        }

        public static ReflectionCommandHandler Create(Type declaringType)
        {
            var handler = Create(declaringType, null, null);
            return handler;
        }

        public static ReflectionCommandHandler Create(MethodInfo methodInfo, object target)
        {
            var handler = Create(methodInfo.DeclaringType, methodInfo, target);
            return handler;
        }

        public static ReflectionCommandHandler Create(Type type, MethodInfo methodInfo, object target = null)
        {
            var handler = new ReflectionCommandHandler(type);
            handler.Binder.SetTarget(target);
            methodInfo = methodInfo ?? GetInvokeMethod(type);
            handler.Binder.SetInvocationMethod(methodInfo);
            return handler;
        }

        public Type TargetType { get; }

        public ReflectionBinder Binder { get; }
        IBinder IBoundCommandHandler.Binder
            => this.Binder;

        public Task<int> InvokeAsync(InvocationContext context)
        {
            // Can we get an easier way to get the handler's owner - which only matters
            // for invocation
            Binder.AddBindingsIfNeeded(context?.ParseResult?.CommandResult?.Command);
            var value = Binder.InvokeAsync(context);
            return CommandHandler.GetResultCodeAsync(value, context);
        }

        private static MethodInfo GetInvokeMethod(Type type)
        {
            var methodInfo = type.GetMethod("InvokeAsync");
            return methodInfo ?? type.GetMethods()
                                     .Where(x => x.Name.StartsWith("Invoke"))
                                     .FirstOrDefault();
        }

    }
}
