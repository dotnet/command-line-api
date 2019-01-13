using System.Reflection;

namespace System.CommandLine.JackFruit
{

    public static class DescriptionStrategies 
    {
        public static  string FromAttribute(Command parent, object source)
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
                    return  null;
            }

             string GetHelp(HelpAttribute attribute, string name)
                => attribute != null
                    ?  attribute.HelpText
                    :  null;
        }
    }
}
