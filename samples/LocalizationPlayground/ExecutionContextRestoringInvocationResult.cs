using System;
using System.CommandLine.Invocation;
using System.Threading;

namespace LocalizationPlayground
{
    internal class ExecutionContextRestoringInvocationResult : IInvocationResult
    {
        private static readonly ContextCallback executionContextApplyCallback = state =>
        {
            var (@this, context) = (ValueTuple<IInvocationResult, InvocationContext>)state!;
            @this.Apply(context);
        };

        private readonly ExecutionContext? executionContext;
        private readonly IInvocationResult innerResult;

        public ExecutionContextRestoringInvocationResult(ExecutionContext? executionContext, IInvocationResult innerResult)
        {
            this.executionContext = executionContext;
            this.innerResult = innerResult;
        }

        public void Apply(InvocationContext context)
        {
            if (executionContext is null)
                innerResult.Apply(context);
            else
                ExecutionContext.Run(executionContext,
                    executionContextApplyCallback,
                    (innerResult, context)
                    );
        }
    }
}
