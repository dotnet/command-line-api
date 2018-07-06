// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    public class MethodBinder
    {
        private readonly MethodInfo _method;
        private readonly object _target;
        private readonly Delegate _delegate;

        public MethodBinder(Delegate @delegate)
        {
            _delegate = @delegate ?? throw new ArgumentNullException(nameof(@delegate));
            _method = _delegate.Method;
        }

        public MethodBinder(MethodInfo method, object target = null)
        {
            _method = method ?? throw new ArgumentNullException(nameof(method));
            _target = target;
        }

        public async Task<int> InvokeAsync(InvocationContext context)
        {
            var arguments = new List<object>();
            var parameters = _method.GetParameters();

            for (var index = 0; index < parameters.Length; index++)
            {
                var parameterInfo = parameters[index];

                var parameterName = parameterInfo.Name;

                if (parameterInfo.ParameterType == typeof(ParseResult))
                {
                    arguments.Add(context.ParseResult);
                }
                else if (parameterInfo.ParameterType == typeof(InvocationContext))
                {
                    arguments.Add(context);
                }
                else if (parameterInfo.ParameterType == typeof(IConsole))
                {
                    arguments.Add(context.Console);
                }
                else
                {
                    var argument = result.CommandResult.ValueForOption(parameterName);
                    arguments.Add(argument);
                }
            }

            object value = null;

            if (_delegate != null)
            {
                value = _delegate.DynamicInvoke(arguments.ToArray());
            }
            else
            {
                value = _method.Invoke(_target, arguments.ToArray());
            }

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
