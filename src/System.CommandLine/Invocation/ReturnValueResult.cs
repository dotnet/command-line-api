// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Invocation
{
    public class ReturnValueResult : IInvocationResult
    {
        private InvocationContext _context;

        public object Value => _context?.InvokeResult;

        public ReturnValueResult()
        {
        }

        public void Apply(InvocationContext context)
        {
            _context = context;
        }
    }
}
