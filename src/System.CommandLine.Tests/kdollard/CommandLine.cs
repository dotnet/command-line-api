using System;
using System.Collections.Generic;
using System.Text;

namespace kdollard
{
    public class CommandLine : Command
    {
        public CommandLine(string name = default,
                 Argument argument = default,
                 Option[] options = default,
                 Command[] subCommands = default)
            : base(name, argument, options, subCommands)
        { }
    }

    public interface IHelpProvider
    {
        string HelpFor(string id);

    }

    public interface IInvocationProvider
    {
        int Invoke(string id, CommandLineResult result);
        Func<CommandLineResult, int> GetInvocation(string id);

    }

    public class CommandLineResult
    {
    }

    public class Help : IHelpProvider
    {
        public string HelpFor(string id)
        {
            switch (id)
            {
                case "dotnet/project/add/projectName":
                    return "The project file to operate on.If a file is not specified, the command will search the current directory for one.";
                case "dotnet/project/add/package":
                    return ".NET Add Package reference Command";
                default:
                    return "Finish this";
            }
        }
    }

    public class Invocation : IInvocationProvider
    {
        public Func<CommandLineResult, int> GetInvocation(string id)
            => throw new NotImplementedException();
        public int Invoke(string id, CommandLineResult result)
        {
            var invocation = GetInvocation(id);
            if (invocation != null)
            {
                return invocation(result);
            }
            return 401;  // figure out correct error handling here 
        }
    }


    public class Arity
    {
        public class NoneAllowed { }
        public class Single : Arity { }
        public class Multiple : Arity { }

        public static Arity.Single ZeroOrOne = new Arity.Single();
        public static Arity.Single ExactlyOne = new Arity.Single();
        public static Arity.Multiple OneToMany = new Arity.Multiple();
        public static Arity.Multiple ZeroToMany = new Arity.Multiple();
    }

    public class Command
    {
        public Command(string name,
                       Argument argument = default,
                       Option[] options = default,
                       Command[] subCommands = default,
                       bool isDefault = default)
        { }

        public Command AddCommand(Command subCommand)
        {
            return this;
        }

        public Command AddCommand(string name,
                        Argument argument = default,
                        Option[] options = default,
                        bool isDefault = default,
                        Command[] subCommands = default)
        {
            return AddCommand(name, argument, options, subCommands);
        }

        public Command AddOption(Option subCommand)
        {
            return this;
        }

        public Command AddOption(string name,
                     string shortCuts = default,
                     Argument argument = default)
        {
            return AddOption(name, shortCuts);
        }

        public Command AddAgument(Argument subCommand)
        {
            return this;
        }

        public Command AddAgument(string name = default,
                        Arity arity = default)
        {
            return AddAgument(name, arity);
        }

    }

    public class Argument
    { }
    public class Argument<T> : Argument
    {
        public Argument(string name = default,
                        Arity arity = default)
        { }
    }

    public class Option
    {
        public Option(string name,
                      string shortCuts = default)
        { }
    }

}
