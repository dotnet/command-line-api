﻿using System.Collections.Generic;

namespace System.CommandLine.JackFruit
{
    public static class Extensions
    {
        public static T AddCommands<T>(this T command, IEnumerable<Command> commands)
            where T : Command
        {
            if (commands == null)
            {
                return command;
            }
            foreach (var subCommand in commands)
            {
                command.AddCommand(subCommand);
            }
            return command;
        }


        public static Command AddOptions(this Command command, IEnumerable<Option> options)
        {
            if (options == null)
            {
                return command;
            }
            foreach (var option in options)
            {
                command.AddOption(option);
            }
            return command;
        }

        public static Option AddAliases(this Option option, IEnumerable<string> aliases)
        {
            if (aliases == null)
            {
                return option;
            }
            foreach (var alias in aliases)
            {
                option.AddAlias (alias);
            }
            return option;
        }

         public static  IEnumerable<string> AliasesFromUnderscores ( string name)
        {
            var aliases = new List<string>();
            aliases.Add(name.Replace("_",""));
            // Note not iterating to end so remaining character is guaranteed
            for (int i = 0; i < name.Length - 1; i++)
            {
                if (name[i] == '_')
                {
                    aliases.Add(name[i + 1].ToString());
                }
            }
            return aliases;
        }

        public static bool AreNamesEqual(this string name1, string name2)
        {
            if (name1.Contains("-"))
            {
                name1 = name1.Replace("-", "");
            }
            if (name2.Contains("-"))
            {
                name2 = name2.Replace("-", "");
            }
            // StringComparison crashes the following line. Switch to that when VS doesn't hate me (I tried to do this right six times)
            return name1.ToLower().Equals(name2.ToLower());
        }
    }
}
