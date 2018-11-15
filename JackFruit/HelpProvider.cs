using System.CommandLine.JackFruit;
using System;
using System.CommandLine;
using System.Collections.Generic;

namespace JackFruit
{
    internal class HelpProvider : IHelpProvider
    {

        public string Help<T>(T command, string name) where T
            : Command 
            => string.IsNullOrWhiteSpace(name) 
                ? CommandHelp(command) 
                : OptionOrArgumentHelp(command, name);

        private string CommandHelp<T>(T command) where T : Command
        {
            switch (command)
            {
                case Tool toolCommand:
                    return "Install or manage tools that extend the .NET experience.";
                default:
                    return "";
            }
        }

        private string OptionOrArgumentHelp<T>(T command, object name) where T : Command
        {
            switch (command)
            {
                case Tool toolCommand:
                    return ToolCommandHelp(name);
                default:
                    return "";
            }
        }

        private string ToolCommandHelp(object name)
        {
            switch (name)
            {
                case "Global":
                    return "Install a tool for use on the command line.";
                default:
                    return "";
            }
        }
    }
}
