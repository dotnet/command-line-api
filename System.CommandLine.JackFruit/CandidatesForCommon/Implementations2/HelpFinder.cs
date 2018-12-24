using System.Reflection;

namespace System.CommandLine.JackFruit
{

    public class HelpFinder : FinderBase<string>
    {
        public HelpFinder(params Approach<string>[] approaches)
            : base(approaches: approaches)
        { }

        protected static (bool, string) HelpFromAttribute(object source, object item)
        {
            switch (item)
            {
                case Object _ when source == item:
                    return HelpFromAttribute(source, null);
                case null when source is Type sourceType:
                    return GetHelp(sourceType.GetCustomAttribute<HelpAttribute>(), sourceType.Name);
                case Type type:
                    return GetHelp(type.GetCustomAttribute<HelpAttribute>(), type.Name);
                case PropertyInfo propertyInfo:
                    return GetHelp(propertyInfo.GetCustomAttribute<HelpAttribute>(), propertyInfo.Name);
                case ParameterInfo parameterInfo:
                    return GetHelp(parameterInfo.GetCustomAttribute<HelpAttribute>(), parameterInfo.Name);
                default:
                    return (false, null);
            }

            (bool, string) GetHelp(HelpAttribute attribute, string name)
                => attribute != null
                    ? (true, attribute.HelpText)
                    : (false, null);
        }

        protected static (bool, string) HelpFromDescription<TSource, TItem>
            (IDescriptionFinder descriptionProvider, TSource source, TItem item)
        {
            var ret = (descriptionProvider != null && (source.Equals(item) || item == null)
                   ? descriptionProvider?.Description(source)
                   : descriptionProvider?.Description(source, NameFromItem()))
                      ?? null;
            return (ret != null, ret);

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

        public static Approach<string> AttributeApproach()
            => Approach<string>.CreateApproach<object, object>(HelpFromAttribute,
                    source=> HelpFromAttribute(source, source));

        public static Approach<string> DescriptionFinderApproach(
                    IDescriptionFinder descriptionFinder = null)
             => Approach<string>.CreateApproach<object, object>(
                 (source, item) => HelpFromDescription(descriptionFinder, source, item),
                 source => HelpFromDescription(descriptionFinder, source, source));

        public static HelpFinder Default() 
            => new HelpFinder(AttributeApproach());
    }
}
