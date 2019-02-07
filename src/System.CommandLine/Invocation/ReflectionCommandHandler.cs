// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    public class ReflectionCommandHandler : ICommandHandler
    {
        public ReflectionCommandHandler(
            Type targetType,
            MethodInfo methodInfo,
            object target = null)
        {
            TargetType = targetType;
            Binder = new ReflectionBinder(TargetType);
            Binder.SetTarget(target);
            Binder.SetInvocationMethod(methodInfo);
        }

        public Type TargetType { get; }

        public ReflectionBinder Binder { get; }

        public Task<int> InvokeAsync(InvocationContext context)
        {
            // Can we get an easier way to get the handler's owner - which only matters
            // for invocation
            var value = Binder.InvokeAsync(context);
            return CommandHandler.GetResultCodeAsync(value, context);
        }
    }
}
