// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;

namespace System.CommandLine.Invocation
{
    public interface IInvocationResult
    {
        void Apply(InvocationContext context);
    }

    internal static class InvocationResultExecutionContextExtensions
    {
        private static readonly ContextCallback ExecutionContextApply = (object state) =>
        {
            var (@this, context) = (ValueTuple<IInvocationResult, InvocationContext>)state;
            @this.Apply(context);
        };

        internal static void ApplyWithExecutionContext(this IInvocationResult result, InvocationContext context)
        {
            ExecutionContext? executionContext = context.ExecutionContext;
            switch (executionContext)
            {
                case null:
                    result.Apply(context);
                    break;
                default:
                    ExecutionContext.Run(executionContext, ExecutionContextApply, (result, context));
                    break;
            }
        }
    }
}
