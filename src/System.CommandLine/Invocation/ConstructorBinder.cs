// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;

namespace System.CommandLine.Invocation
{
    public class ConstructorBinder : MethodBinderBase
    {
        private readonly ConstructorInfo _constructor;

        public ConstructorBinder(ConstructorInfo constructor) 
            : base(constructor)
        {
            _constructor = constructor;
        }

        protected override object InvokeMethod(object[] arguments)
        {
            return _constructor.Invoke(arguments);
        }
    }
}
