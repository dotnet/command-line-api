// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    internal class TypeBindingCommandHandler : CommandHandler
    {
        private readonly MethodInfo _onExecuteMethodInfo;
        private readonly TypeBinder _typeBinder;

        public TypeBindingCommandHandler(
            MethodInfo method,
            TypeBinder typeBinder)
        {
            _onExecuteMethodInfo = method ?? throw new ArgumentNullException(nameof(method));
            _typeBinder = typeBinder;
        }

        public override Task<int> InvokeAsync(InvocationContext context)
        {
            var instance =
                _onExecuteMethodInfo.IsStatic
                    ? null
                    : _typeBinder.CreateInstance(context);

            var args = Binder.GetMethodArguments(
                context,
                _onExecuteMethodInfo.GetParameters());

            var value = _onExecuteMethodInfo.Invoke(instance, args);

            return GetResultCodeAsync(value);
        }
    }
}
