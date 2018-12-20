using System.CommandLine.Invocation;
using System.Linq;
using System.Reflection;

namespace System.CommandLine.JackFruit
{
    public class PropertyInfoOptionBinder : IOptionBinder<Type, PropertyInfo>
    {
        public IHelpProvider<Type> HelpProvider { get; set; }

        public string GetHelp(Type parentType, PropertyInfo propertyInfo)
            => HelpProvider?.GetHelp(parentType, propertyInfo);

        public string GetName(Type parentType, PropertyInfo propertyInfo)
            => propertyInfo.Name;

        public Option GetOption(Type parentType, PropertyInfo propertyInfo)
        {
            var option = TypeBinder.BuildOption(propertyInfo);
            option.Description = GetHelp(parentType, propertyInfo);
            var aliasAttribute = propertyInfo.GetCustomAttribute<AliasAttribute>();
            if (aliasAttribute != null)
            {
                foreach (var alias in aliasAttribute.Aliases)
                {
                    option.AddAlias(alias);
                }
            }
            else
            {
                option.AddAliases( Extensions.AliasesFromUnderscores(option.Name));
                option.Name = option.Aliases.First();
            }
            return option;
        }

    }
}
