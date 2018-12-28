using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace System.CommandLine.JackFruit
{

    public class AliasFinder : FinderBaseForList<AliasFinder, string>
    {
        protected static (bool, IEnumerable<string>) FromAttribute(Command[] parents, object source)
        {
            switch (source)
            {
                case Type type:
                    return GetName(type.GetCustomAttribute<AliasAttribute>(), type.Name);
                case MethodInfo methodInfo:
                    return GetName(methodInfo.GetCustomAttribute<AliasAttribute>(), methodInfo.Name);
                case PropertyInfo propertyInfo:
                    return GetName(propertyInfo.GetCustomAttribute<AliasAttribute>(), propertyInfo.Name);
                case ParameterInfo parameterInfo:
                    return GetName(parameterInfo.GetCustomAttribute<AliasAttribute>(), parameterInfo.Name);
                default:
                    return (false, null);
            }

            (bool, IEnumerable<string>) GetName(AliasAttribute attribute, string name)
                => attribute != null
                    ? (true, attribute.Aliases)
                    : (false, new string[] { name });
        }

        // TODO: Add approach for underscore. Anything else?

        public static AliasFinder Default()
            => new AliasFinder()
                    .SetFinalTransform(x => x.Select(n => n.ToKebabCase().ToLower()))
                    .AddApproachFromFunc<object>(FromAttribute);
    }
}
