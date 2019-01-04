using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace System.CommandLine.JackFruit
{

    public static class AliasStrategies
    {
        public static (bool, IEnumerable<string>) FromAttribute(Command[] parents, object source)
        {
            switch (source)
            {
                case Type type:
                    return GetNames(type.GetCustomAttributes<AliasAttribute>(), type.Name);
                case MethodInfo methodInfo:
                    return GetNames(methodInfo.GetCustomAttributes<AliasAttribute>(), methodInfo.Name);
                case PropertyInfo propertyInfo:
                    return GetNames(propertyInfo.GetCustomAttributes<AliasAttribute>(), propertyInfo.Name);
                case ParameterInfo parameterInfo:
                    return GetNames(parameterInfo.GetCustomAttributes<AliasAttribute>(), parameterInfo.Name);
                default:
                    return (false, null);
            }

            (bool, IEnumerable<string>) GetNames(IEnumerable<AliasAttribute> attributes, string name)
            {
                var candidates = attributes.SelectMany(a => a.Aliases).ToList();
                if (!candidates.Contains(name))
                {
                    candidates.Insert(0, name);
                }
                return (false, candidates);
            }
        }

        // TODO: Add approach for underscore. Anything else? Maybe XML Docs
    }
}
