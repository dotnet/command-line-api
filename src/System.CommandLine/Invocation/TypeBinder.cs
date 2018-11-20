using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace System.CommandLine.Invocation
{
    public class TypeBinder
    {
        private readonly Type _type;
        private IReadOnlyCollection<PropertyInfo> _settableProperties;
        private readonly ConstructorBinder _constructorBinder;

        public TypeBinder(
            Type type,
            ConstructorBinder constructorBinder = null)
        {
            _type = type ?? throw new ArgumentNullException(nameof(type));

            _constructorBinder =
                constructorBinder ??
                new ConstructorBinder(_type.GetConstructors().SingleOrDefault() ??
                                      throw new ArgumentException($"No eligible constructor found to bind type {_type}"));
        }

        public object CreateInstance(InvocationContext context)
        {
            var instance = _constructorBinder.InvokeMethod(context);

            SetProperties(context, instance);

            return instance;
        }

        public void SetProperties(
            InvocationContext context,
            object instance)
        {
            foreach (var propertyInfo in GetSettableProperties())
            {
                SetProperty(context, instance, propertyInfo);
            }
        }

        private static void SetProperty(InvocationContext context, object instance, PropertyInfo propertyInfo)
        {
            if (propertyInfo.PropertyType == typeof(ParseResult))
            {
                propertyInfo.SetValue(instance, context.ParseResult);
            }
            else if (propertyInfo.PropertyType == typeof(InvocationContext))
            {
                propertyInfo.SetValue(instance, context);
            }
            else if (propertyInfo.PropertyType == typeof(IConsole))
            {
                propertyInfo.SetValue(instance, context.Console);
            }
            else
            {
                if (Binder.TryGetValue(context, propertyInfo.Name, out object value))
                {
                    propertyInfo.SetValue(instance, value);
                }
            }
        }

        public IEnumerable<Option> BuildOptions()
        {
            var optionSet = new SymbolSet();

            foreach (var parameter in _constructorBinder.BuildOptions())
            {
                optionSet.Add(parameter);
            }

            foreach (var property in GetSettableProperties()
                .OmitInfrastructureTypes())
            {
                var option = BuildOption(property);

                if (!optionSet.Contains(option.Name))
                {
                    optionSet.Add(option);
                }
            }

            return optionSet.Cast<Option>();
        }

        public static Option BuildOption(PropertyInfo property)
            => property.BuildOption();

        private IEnumerable<PropertyInfo> GetSettableProperties()
        {
            return _settableProperties ??
                   (_settableProperties = _type.GetProperties().Where(p => p.CanWrite).ToArray());
        }
    }
}
