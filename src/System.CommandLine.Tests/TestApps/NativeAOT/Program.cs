using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;

public class Program
{
    private static int Main(string[] args)
    {
        Option<bool> boolOption = new Option<bool>(new[] { "--bool", "-b" }, "Bool option");
        Option<string> stringOption = new Option<string>(new[] { "--string", "-s" }, "String option");

        RootCommand command = new RootCommand
        {
            boolOption,
            stringOption
        };

        command.SetHandler(Run);

        return new CommandLineBuilder(command).Build().Invoke(args);

        void Run(InvocationContext context)
        {
            context.Console.WriteLine($"Bool option: {context.ParseResult.GetValueForOption(boolOption)}");
            context.Console.WriteLine($"String option: {context.ParseResult.GetValueForOption(stringOption)}");
        }
    }
}