// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    internal class MethodBindingCommandHandler : ICommandHandler
    {
        private readonly object _target;
        private readonly MethodInfo _method;
        private readonly Delegate _delegate;

        public MethodBindingCommandHandler(Delegate @delegate)
        {
            _delegate = @delegate ?? throw new ArgumentNullException(nameof(@delegate));
            _method = @delegate.Method;
        }

        public MethodBindingCommandHandler(MethodInfo method, object target = null)
        {
            _method = method;
            _target = target;
        }

        public Task<int> InvokeAsync(InvocationContext context)
        {
            var parameters = _method.GetParameters();

            var arguments = Binder.GetMethodArguments(context, parameters);

            object value = null;

            try
            {
                if (_delegate != null)
                {
                    value = _delegate.DynamicInvoke(arguments);
                }
                else
                {
                    value = _method.Invoke(_target, arguments);
                }
            }
            catch (TargetInvocationException te)
            {
                if (te.InnerException != null)
                {
                    throw te.InnerException;
                }
                throw;
            }

            return CommandHandler.GetResultCodeAsync(value);
        }
    }
}
