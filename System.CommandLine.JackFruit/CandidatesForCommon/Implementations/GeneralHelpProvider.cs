using System.Reflection;

namespace System.CommandLine.JackFruit
{

    public class HelpProvider<TCommandSource>
        : IHelpProvider<TCommandSource>
    {
        private readonly Func<TCommandSource, object, string>[] approaches;

        public HelpProvider(params Func<TCommandSource, object, string>[] approaches)
        {
            this.approaches = approaches;
        }

        public string GetHelp(TCommandSource source)
            => GetHelp(source, source);

        public string GetHelp<TItem>(TCommandSource source, TItem item)
        {
            string candidate = null;
            foreach (var approach in approaches)
            {
                candidate = approach(source, item);
                if (candidate != null)
                {
                    break;
                }
            }
            return candidate;
        }

        protected static string HelpFromAttribute(TCommandSource source, object item)
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
            {
                return attribute?.HelpText;
            }
        }

        protected static string HelpFromDescription(IDescriptionProvider<TCommandSource> descriptionProvider, TCommandSource source, object item)
        {
            return (descriptionProvider != null && (source.Equals(item) || item == null)
                   ? descriptionProvider?.Description(source)
                   : descriptionProvider?.Description(source, NameFromItem()))
                      ?? "";
            string NameFromItem()
            {
                switch (item)
                {
                    case PropertyInfo p:
                        return p.Name;
                    case ParameterInfo p:
                        return p.Name;
                    default:
                        return null;
                }
            }
        }

        public static HelpProvider<TCommandSource> GeneralHelpProvider(IDescriptionProvider<TCommandSource> descriptionProvider = null)
        {
            return new HelpProvider<TCommandSource>(HelpFromAttribute, 
                (source,item)=>HelpFromDescription(descriptionProvider,source, item));
        }
    }
}
