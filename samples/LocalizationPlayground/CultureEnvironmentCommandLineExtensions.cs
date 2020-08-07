using System;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Globalization;
using System.Threading;

namespace LocalizationPlayground
{
    internal static class CultureEnvironmentCommandLineExtensions
    {
        internal static CommandLineBuilder UseCultureEnvironment(
            this CommandLineBuilder builder)
        {
            return builder.UseMiddleware(async (context, next) =>
            {
                if (Environment.GetEnvironmentVariable("DOTNET_SYSTEM_GLOBALIZATION_CULTURE") is string culture &&
                        !string.IsNullOrEmpty(culture))
                    CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(culture);

                await next(context).ConfigureAwait(false);

                if (context.InvocationResult is IInvocationResult innerResult)
                {
                    var execCtx = ExecutionContext.Capture();
                    context.InvocationResult = new ExecutionContextRestoringInvocationResult(execCtx, innerResult);
                }
            }, MiddlewareOrder.ExceptionHandler);
        }
    }
}
