// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Invocation
{
    public class DelegateBinder : MethodBinderBase
    {
        private readonly Delegate _delegate;

        public DelegateBinder(Delegate @delegate) :
            base(@delegate?.Method ?? throw new ArgumentNullException(nameof(@delegate)))
        {
            _delegate = @delegate;
        }

        protected override object InvokeMethod(object[] arguments) =>
            _delegate.DynamicInvoke(arguments);
    }
}
