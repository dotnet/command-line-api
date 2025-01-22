using System;
using System.CommandLine;
using System.CommandLine.Parsing;

public class Program
{
    private static int Main(string[] args)
    {
        Option<bool> boolOption = new ("--bool", "-b") { Description = "Bool option" };
        Option<string> stringOption = new ("--string", "-s") { Description = "String option" };

        RootCommand command = new ()
        {
            boolOption,
            stringOption
        };

        command.SetAction(Run);

        return new CommandLineConfiguration(command).Invoke(args);

        void Run(ParseResult parseResult)
        {
            Console.WriteLine($"Bool option: {parseResult.GetValue(boolOption)}");
            Console.WriteLine($"String option: {parseResult.GetValue(stringOption)}");
        }
    }
}