using System;
using System.Collections.Generic;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Text;

namespace System.CommandLine.JackFruit
{
    public static class Extensions
    {
        public static T AddCommands<T>(this T commandBuilder, IEnumerable<Command> commands)
            where T: CommandBuilder // could be CommandBulider or CommandLineBuilder
        {
            foreach (var command in commands)
            {
                commandBuilder.AddCommand(command);
            }
            return commandBuilder;
        }


        public static CommandLineBuilder AddStandardDirectives(this CommandLineBuilder builder)
            => builder
                .UseDebugDirective()
                .UseParseErrorReporting()
                .UseParseDirective()
                .UseHelp()
                .UseSuggestDirective()
                .RegisterWithDotnetSuggest()
                .UseExceptionHandler();
    }
}
