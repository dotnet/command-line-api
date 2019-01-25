// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    public class TypeCreationCommandHandler : ICommandHandler
    {
        private readonly MethodInfo _onExecuteMethodInfo;
        private readonly TypeBinder _typeBinder;

        public TypeCreationCommandHandler(
            MethodInfo method,
            TypeBinder typeBinder)
        {
            _onExecuteMethodInfo = method ?? throw new ArgumentNullException(nameof(method));
            _typeBinder = typeBinder;
        }

        public Task<int> InvokeAsync(InvocationContext context)
        {
            var methodBinder = new MethodBinder(
                _onExecuteMethodInfo, 
                () => _typeBinder.CreateInstance(context));

            return methodBinder.InvokeAsync(context);
        }
    }
}
