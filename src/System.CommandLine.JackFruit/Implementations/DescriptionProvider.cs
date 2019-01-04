using System.Reflection;

namespace System.CommandLine.JackFruit
{

    public class DescriptionProvider : FinderBase<DescriptionProvider, string>
    {
        protected static (bool, string) FromAttribute(Command[] parents, object source)
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

        public static DescriptionProvider Default()
            => new DescriptionProvider()
                .AddApproach<object>(FromAttribute);

        public DescriptionProvider AddDescriptionFinder(Func<object, string> descriptionFinder)
        {
            AddApproach<object>(
                   (parents, source) => (false, descriptionFinder(source)));
            return this;
        }
    }
}
