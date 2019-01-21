using System.Reflection;

namespace System.CommandLine.Invocation
{
    public class PropertyBindingSide : BindingSide
    {
        private PropertyBindingSide(PropertyInfo propertyInfo)
            : base(GetGetter(propertyInfo), GetSetter(propertyInfo))
            => PropertyInfo = propertyInfo;

        public static PropertyBindingSide Create(PropertyInfo propertyInfo)
            => new PropertyBindingSide(propertyInfo);

        public PropertyInfo PropertyInfo { get; }

        private static BindingGetter GetGetter(PropertyInfo propertyInfo)
            => (context, target) => propertyInfo.GetValue(target);

        private static BindingSetter GetSetter(PropertyInfo propertyInfo)
            => (context, target, value) => propertyInfo.SetValue(target, value);
    }

    public class ParameterBindingSide : BindingSide
    {
        private ParameterBindingSide(ParameterInfo parameterInfo, ParameterCollection parameterCollection)
            : base(GetGetter(parameterInfo, parameterCollection), 
                  GetSetter(parameterInfo, parameterCollection))
            => ParameterInfo = parameterInfo;

        public static ParameterBindingSide Create(ParameterInfo parameterInfo, ParameterCollection parameterCollection)
        {
            if (parameterCollection == null)
            {
                throw new ArgumentNullException(nameof(parameterCollection));
            }

            return new ParameterBindingSide(parameterInfo, parameterCollection);
        }

        public ParameterInfo ParameterInfo { get; }

        private static BindingGetter GetGetter(ParameterInfo parameterInfo, ParameterCollection parameterColection)
        {
            parameterColection = parameterColection
                                ?? new ParameterCollection(parameterInfo.Member as MethodBase);
            return (context, target) => parameterColection.GetParameter(parameterInfo);
        }

        private static BindingSetter GetSetter(ParameterInfo parameterInfo, ParameterCollection parameterColection)
        {
            parameterColection = parameterColection
                           ?? new ParameterCollection(parameterInfo.Member as MethodBase);
            return (context, target, value) => parameterColection.SetParameter(parameterInfo, value);
        }
    }
}
