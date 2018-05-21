// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Invocation
{
    public class MethodBinder
    {
        private readonly Delegate _delegate;

        public MethodBinder(Delegate @delegate)
        {
            _delegate = @delegate ?? throw new ArgumentNullException(nameof(@delegate));
        }

        public void Invoke(ParseResult result)
        {
            var arguments = new List<object>();
            var parameters = _delegate.Method.GetParameters();

            for (var index = 0; index < parameters.Length; index++)
            {
                var parameterName = parameters[index].Name;
                var argument = result.Command().ValueForOption(parameterName);
                arguments.Add(argument);
            }

            _delegate.DynamicInvoke(arguments.ToArray());
        }
    }
}
