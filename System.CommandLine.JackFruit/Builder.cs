using System;
using System.Collections.Generic;
using System.Text;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;

namespace System.CommandLine.JackFruit
{
    internal static class BuilderTools
    {
        internal static CommandLineBuilder AddStandardDirectives(this CommandLineBuilder builder)
            => builder
                .UseDebugDirective()
                .UseParseErrorReporting()
                .UseParseDirective()
                .UseHelp()
                .UseSuggestDirective()
                .RegisterWithDotnetSuggest()
                .UseExceptionHandler();

        internal static CommandLineBuilder Create<TCli, THelper>() => throw new NotImplementedException();
    }
}
