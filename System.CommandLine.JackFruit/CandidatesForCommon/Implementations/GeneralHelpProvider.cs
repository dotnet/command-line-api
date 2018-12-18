using System.Reflection;

namespace System.CommandLine.JackFruit
{
    public class GeneralHelpProvider<TCommandSource>
        : IHelpProvider<TCommandSource>
    {
        private readonly IDescriptionProvider<TCommandSource> descriptionProvider;
        private string helpText;

        public GeneralHelpProvider(IDescriptionProvider<TCommandSource> descriptionProvider = null)
        {
            this.descriptionProvider = descriptionProvider;
        }

        public string GetHelp(TCommandSource source)
        {
            return helpText = HelpFromAttributeOrDescriptionProvider(source, source);
        }

        public string GetHelp<TOptionSource>(TCommandSource source, TOptionSource optionSource)
        {
            return helpText = HelpFromAttributeOrDescriptionProvider(source, optionSource);
        }

        private string HelpFromAttributeOrDescriptionProvider(TCommandSource source, object item)
        {
            switch (item)
            {
                case Type type:
                    return GetHelp(type.GetCustomAttribute<HelpAttribute>(), type.Name);
                case PropertyInfo propertyInfo:
                    return GetHelp(propertyInfo.GetCustomAttribute<HelpAttribute>(), propertyInfo.Name);
                case ParameterInfo parameterInfo:
                    return GetHelp(parameterInfo.GetCustomAttribute<HelpAttribute>(), parameterInfo.Name);
                default:
                    return null;
            }
            string GetHelp(HelpAttribute attribute, string name)
                => attribute?.HelpText
                    ?? (source.Equals(item)
                        ? descriptionProvider?.Description(source)
                        : descriptionProvider?.Description(source, name))
                           ?? "";
        }
    }
}
