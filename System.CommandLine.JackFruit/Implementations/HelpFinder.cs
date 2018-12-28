using System.Reflection;

namespace System.CommandLine.JackFruit
{

    public class HelpFinder : FinderBase<HelpFinder, string>
    {
        protected static (bool, string) FromAttribute(Command parent, object source)
        {
            switch (source)
            {
                // This will need more work when source is a tuple with type/name, etc
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
                    ? (false, attribute.HelpText)
                    : (false, null);
        }

        protected static (bool, string) FromDescription
            (IDescriptionFinder descriptionProvider, Command parent, object source)
        {
            if (descriptionProvider == null)
            {
                return (false, null);
            }
            switch (source)
            {
                case ValueTuple<object, object> tuple:
                    var ret = ( (source.Equals(tuple.Item1) || tuple.Item2 == null)
                                      ? descriptionProvider?.Description(source)
                                      : descriptionProvider?.Description(source, NameFromItem(tuple.Item2)))
                                         ?? null;
                    return (false, ret);
                default:
                    ret =  descriptionProvider?.Description(source);
                    return (false, ret);
            }

            string NameFromItem(object item)
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

        public static HelpFinder Default()
            => new HelpFinder()
                .AddApproachFromFunc<object>(FromAttribute);


        public HelpFinder AddDescriptionFinder(IDescriptionFinder descriptionFinder)
        {
            AddApproachFromFunc<object>(
                   (parent, source) => FromDescription(descriptionFinder, parent, source));
            return this;
        }
    }
}
