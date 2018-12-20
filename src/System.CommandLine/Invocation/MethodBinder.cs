// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;

namespace System.CommandLine.Invocation
{
    public class MethodBinder : MethodBinderBase
    {
        private readonly Func<object> _getTarget;

        public MethodBinder(
            MethodInfo method,
            Func<object> getTarget = null) :
            base(method)
        {
            _getTarget = getTarget;
        }

        protected override object InvokeMethod(object[] arguments) =>
            Method.Invoke(_getTarget?.Invoke(), arguments);
    }
}
