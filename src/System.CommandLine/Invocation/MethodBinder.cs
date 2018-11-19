// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;

namespace System.CommandLine.Invocation
{
    public class MethodBinder : MethodBinderBase
    {
        private readonly object _target;

        public MethodBinder(MethodInfo method, object target = null) :
            base(method)
        {
            _target = target;
        }

        protected override object InvokeMethod(object[] arguments) =>
            Method.Invoke(_target, arguments);
    }
}
