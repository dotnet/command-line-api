using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;

public class Program
{
    static void Run(bool boolean, string text)
    {
        Console.WriteLine($"Bool option: {text}");
        Console.WriteLine($"String option: {boolean}");
    }

    private static int Main(string[] args)
    {
        Option<bool> boolOption = new Option<bool>(new[] { "--bool", "-b" }, "Bool option");
        Option<string> stringOption = new Option<string>(new[] { "--string", "-s" }, "String option");

        RootCommand command = new RootCommand
        {
            boolOption,
            stringOption
        };

        command.SetHandler<bool, string>(Run, boolOption, stringOption);

        return new CommandLineBuilder(command).Build().Invoke(args);
    }
}