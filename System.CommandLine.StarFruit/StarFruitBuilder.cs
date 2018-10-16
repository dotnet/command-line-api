using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace System.CommandLine.StarFruit
{
    public class StarFruitBuilder
    {
        public static CommandDefinition Build<T>()
            where T : Command
        {
            return GetCommandInfo(typeof(T));
        }

        private class CommandInfo
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public ArgumentDefinition Arguments { get; set; }
            public IReadOnlyCollection<SymbolDefinition> ChildSymbols => new ReadOnlyCollection<SymbolDefinition>(_childSymbols.ToList());
            private IEnumerable<SymbolDefinition> _childSymbols = new List<SymbolDefinition>();
            public void AddChildSymbols(IEnumerable<SymbolDefinition> childSymbols) => _childSymbols = childSymbols;
        }

        private static CommandDefinition GetCommandInfo(Type type)
        {
            var commandInfo = new CommandInfo();
            // This throws because I do not anticipate this occurring past development
            CommandAttribute commandAttribute = type.GetCustomAttributes<CommandAttribute>()
                                    .Single();
            var name = commandAttribute.Name
                                ?? type.Name;
            var description = commandAttribute.Description;
            var arguments = GetCommandArgumentDefinition(type);
            var childSymbols = GetChildSymbols(type);
            // KAD: TODO: stop ignoring the children!
            return new CommandDefinition(name, description, new ReadOnlyCollection<SymbolDefinition>(childSymbols.ToList()), arguments);
        }

        private static IEnumerable<SymbolDefinition> GetChildSymbols(Type type)
        {
            var options = type.GetProperties()
                            .Select<PropertyInfo, (PropertyInfo propertyInfo, OptionAttribute optionAttribute)>(p => (p, p.GetCustomAttributes<OptionAttribute>().FirstOrDefault()))
                            .Where(p => p.optionAttribute != null)
                            .Select(t => GetChildOption(t.propertyInfo, t.optionAttribute));

            var subCommands = type.GetNestedTypes()
                              .Where(c => typeof(Command).IsAssignableFrom(c))
                              .Select(c => (c))
                              .Select(c => GetCommandInfo(c));
            return options.Union<SymbolDefinition>(subCommands);
        }

        private static OptionDefinition GetChildOption(PropertyInfo propertyInfo, OptionAttribute optionAttribute)
        {
            var name = string.IsNullOrEmpty(optionAttribute.Name)
                       ? SeparateWordsWithDashes(propertyInfo.Name)
                       : optionAttribute.Name;
            var aliases = new string[] { name }.Union(optionAttribute.Aliases).ToArray();

            ArgumentDefinition argumentDefinition = null;
            if (optionAttribute is OptionWithArgumentAttribute optionWithArgumentAttribute)
            {
                argumentDefinition = new ArgumentDefinition(optionWithArgumentAttribute.Arity);
            }

            return new OptionDefinition(aliases, optionAttribute.Description, argumentDefinition);
        }

        // KAD: Much more work here
        private static ArgumentDefinition GetCommandArgumentDefinition(Type type)
        {
            var (propertyInfo, commandArgumentAttribute) = type.GetProperties()
                            .Select<PropertyInfo, (PropertyInfo propertyInfo, CommandArgumentAttribute commandArgumentAttribute)>(p => (p, p.GetCustomAttributes<CommandArgumentAttribute>().FirstOrDefault()))
                            .Where(p => p.commandArgumentAttribute != null)
                            .FirstOrDefault();
            return commandArgumentAttribute == null
                    ? null
                    : new ArgumentDefinition(commandArgumentAttribute.Arity);
        }

        // TODO: Write this
        private static string SeparateWordsWithDashes(string name) => name;
    }
}
