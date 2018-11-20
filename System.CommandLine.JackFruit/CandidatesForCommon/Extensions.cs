using System;
using System.Collections.Generic;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Linq;
using System.Reflection;
using System.Text;

namespace System.CommandLine.JackFruit
{
    public static class Extensions
    {
        //public static T AddCommands<T>(this T commandBuilder, IEnumerable<Command> commands)
        //    where T: CommandBuilder // could be CommandBulider or CommandLineBuilder
        //{
        //    foreach (var command in commands)
        //    {
        //        commandBuilder.AddCommand(command);
        //    }
        //    return commandBuilder;
        //}

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

        public static CommandLineBuilder AddStandardDirectives(this CommandLineBuilder builder)
            => builder
                .UseDebugDirective()
                .UseParseErrorReporting()
                .UseParseDirective()
                .UseHelp()
                .UseSuggestDirective()
                .RegisterWithDotnetSuggest()
                .UseExceptionHandler();

        public static bool IgnoreProperty(this PropertyInfo propertyInfo)
             => propertyInfo
                .CustomAttributes
                .Any(x => x.AttributeType == typeof(IgnoreAttribute));


        // TODO: This is a helper method on string - an extension pollutes Intellisense. Where to put it?
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

        // TODO: This is a helper method on string - an extension pollutes Intellisense. Where to put it?
        public static string GetHelp<TSource, TItem>(HelpAttribute attribute,
                IHelpProvider<TSource, TItem> helpProvider, TSource source, TItem item)
        {
            if (attribute != null)
            {
                return attribute.HelpText;
            }
            return helpProvider != null
                ? helpProvider.GetHelp(source, item)
                : "";
        }

        // TODO: This is a helper method on string - an extension pollutes Intellisense. Where to put it?
        public static string GetHelp<TSource>(HelpAttribute attribute,
                IHelpProvider<TSource> helpProvider, TSource source)
        {
            if (attribute != null)
            {
                return attribute.HelpText;
            }
            return helpProvider != null
                ? helpProvider.GetHelp(source)
                : "";
        }

        // TODO: This is a helper method on string - an extension pollutes Intellisense. Where to put it?
        public static string GetHelp<TSource>(HelpAttribute attribute,
                IHelpProvider<TSource> helpProvider, TSource source, string name)
        {
            if (attribute != null)
            {
                return attribute.HelpText;
            }
            return helpProvider != null
                ? helpProvider.GetHelp(source, name)
                : "";
        }

    }
}
