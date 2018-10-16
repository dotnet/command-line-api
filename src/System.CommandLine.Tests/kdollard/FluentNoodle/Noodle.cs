using System;
using System.Collections.Generic;
using System.Text;

namespace System.CommandLine.Tests.kdollard.FluentNoodle
{

    public class BaseThing
    {

    }
    public class Command : BaseThing
    {

    }
    public class Argument : BaseThing
    {

    }
    public class Option : BaseThing
    {

    }

    public static class CommandExtensions
    {


        public static IEnumerable<Command> AddCommand(this IEnumerable<Command> commandStack, string Name)
        {
            return commandStack; //.WithCommand( new Command());
        }

        public static IEnumerable<Command> AddOption(this IEnumerable<Command> commandStack, string Name)
        {
            return commandStack;//.WithOption(new Option());
        }

        public static IEnumerable<Command> AddArgument(this IEnumerable<Command> commandStack, string Name)
        {
            return commandStack;//.WithArgument(new Argument());
        }

        public static IEnumerable<Command> AddSubCommand(this IEnumerable<Command> commandStack, string Name)
        {
            return commandStack;//.WithCommand(new Command());
        }
    }

}
