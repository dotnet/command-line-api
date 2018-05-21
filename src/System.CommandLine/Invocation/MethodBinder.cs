// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Invocation
{
    public class MethodBinder
    {
        private readonly Delegate _delegate;
        private readonly string[] _optionAliases;

        public MethodBinder(Delegate @delegate, params string[] optionAliases)
        {
            _delegate = @delegate ?? throw new ArgumentNullException(nameof(@delegate));
            _optionAliases = optionAliases;
        }

        public void Invoke(ParseResult result)
        {
            var arguments = new List<object>();
            var parameters = _delegate.Method.GetParameters();
            for (var index = 0; index < parameters.Length; index++)
            {
                var argument = result.Command().ValueForOption(_optionAliases[index]);
                arguments.Add(argument);
            }

            _delegate.DynamicInvoke(arguments.ToArray());
        }
    }
}
