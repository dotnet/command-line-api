// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    internal class MethodBindingCommandHandler : ICommandHandler
    {
        private readonly MethodBinderBase _methodBinder;

        public MethodBindingCommandHandler(Delegate @delegate)
        {
            _methodBinder = new DelegateBinder(@delegate);
        }

        public MethodBindingCommandHandler(MethodInfo method, object target = null)
        {
            _methodBinder = new MethodBinder(method, target);
        }

        public Task<int> InvokeAsync(InvocationContext context)
        {
            return  _methodBinder.InvokeAsync(context);
        }
    }
}
