using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;

public class Program
{
    private static int Main(string[] args)
    {
        Option<bool> boolOption = new Option<bool>("--bool", "-b") { Description = "Bool option" };
        Option<string> stringOption = new Option<string>("--string", "-s") { Description = "String option" };

        RootCommand command = new RootCommand
        {
            boolOption,
            stringOption
        };

        command.SetAction(Run);

        return new CommandLineConfiguration(command).Invoke(args);

        void Run(InvocationContext context)
        {
            Console.WriteLine($"Bool option: {context.ParseResult.GetValue(boolOption)}");
            Console.WriteLine($"String option: {context.ParseResult.GetValue(stringOption)}");
        }
    }
}