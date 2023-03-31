using System;
using System.CommandLine;
using System.CommandLine.Parsing;

public class Program
{
    private static int Main(string[] args)
    {
        CliOption<bool> boolOption = new ("--bool", "-b") { Description = "Bool option" };
        CliOption<string> stringOption = new ("--string", "-s") { Description = "String option" };

        CliRootCommand command = new ()
        {
            boolOption,
            stringOption
        };

        command.SetAction(Run);

        return new CliConfiguration(command).Invoke(args);

        void Run(ParseResult parseResult)
        {
            Console.WriteLine($"Bool option: {parseResult.GetValue(boolOption)}");
            Console.WriteLine($"String option: {parseResult.GetValue(stringOption)}");
        }
    }
}