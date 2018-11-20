using System.Reflection;

namespace System.CommandLine.JackFruit
{
    public class TypeHelpProvider : IHelpProvider<Type, PropertyInfo>
    {
        private readonly IDescriptionProvider<Type> descriptionProvider;

        public TypeHelpProvider(IDescriptionProvider<Type> descriptionProvider = null)
        {
            this.descriptionProvider = descriptionProvider;
        }

        public string GetHelp(Type source)
        {
            var attribute = source.GetCustomAttribute<HelpAttribute>(); ;
            if (attribute != null)
            {
                return attribute.HelpText;
            }
            return descriptionProvider != null 
                ? descriptionProvider.Description(source) 
                : "";
        }

        public string GetHelp(Type source,  PropertyInfo propertyInfo)
        {
            var attribute = propertyInfo.GetCustomAttribute<HelpAttribute>(); ;
            if (attribute != null)
            {
                return attribute.HelpText;
            }
            return descriptionProvider != null 
                ? descriptionProvider.Description(source, propertyInfo.Name) 
                : "";
        }

        public string GetHelp(Type source, string name) 
            => descriptionProvider != null
                ? descriptionProvider.Description(source, name) 
                : "";
    }
}
