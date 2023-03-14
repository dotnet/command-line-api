﻿using System;
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

        command.SetHandler(Run);

        return new CommandLineBuilder(command).Build().Invoke(args);

        int Run(InvocationContext context)
        {
            context.Console.WriteLine($"Bool option: {context.ParseResult.GetValue(boolOption)}");
            context.Console.WriteLine($"String option: {context.ParseResult.GetValue(stringOption)}");

            return 0;
        }
    }
}