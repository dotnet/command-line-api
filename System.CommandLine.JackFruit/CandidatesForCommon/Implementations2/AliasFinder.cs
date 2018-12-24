using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace System.CommandLine.JackFruit
{

    public class AliasFinder : FinderForListBase<string>
    {
        public AliasFinder(Func<object, object> initialCheck = null,
                           Func<IEnumerable<string>, IEnumerable<string>> finalTransform = null, 
                           params Approach<IEnumerable<string>>[] approaches)
              : base(initialCheck, finalTransform, approaches)
        { }

        protected static (bool, IEnumerable<string>) AliasesFromAttribute(object source, object item)
        {
            switch (source)
            {
                case Type type:
                    return GetName(type.GetCustomAttribute<AliasAttribute>(), type.Name);
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

        public static Approach<IEnumerable<string>> AttributeApproach()
            => Approach<IEnumerable<string>>.CreateApproach<object, object>(AliasesFromAttribute,
                    source=>AliasesFromAttribute(source, source));

        private static IEnumerable<string> TransformNames(IEnumerable<string> names) 
            => names.Select(TransformName);

        private static string TransformName(string name) 
            => name.ToKebabCase().ToLower();

        public static AliasFinder Default()
            => new AliasFinder(null, TransformNames, AttributeApproach());
    }
}
