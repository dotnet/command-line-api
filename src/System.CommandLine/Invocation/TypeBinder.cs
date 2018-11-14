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

        public TypeBinder(Type type)
        {
            _type = type ?? throw new ArgumentNullException(nameof(type));
            _constructorBinder = new ConstructorBinder(GetConstructor());
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
                    var argument = context.ParseResult
                                          .CommandResult
                                          .ValueForOption(
                                              Binder.FindMatchingOptionName(
                                                  context.ParseResult,
                                                  propertyInfo.Name));
                    propertyInfo.SetValue(instance, argument);
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
                var option = property.BuildOption();

                if (!optionSet.Contains(option.Name))
                {
                    optionSet.Add(option);
                }
            }

            return optionSet.Cast<Option>();
        }

        private IEnumerable<PropertyInfo> GetSettableProperties()
        {
            return _settableProperties ??
                   (_settableProperties = _type.GetProperties().Where(p => p.CanWrite).ToArray());
        }

        protected virtual ConstructorInfo GetConstructor()
        {
            // TODO: Clean up to consider multiple constructors
            return
                _type.GetConstructors().SingleOrDefault() ??
                throw new ArgumentException($"No eligible constructor found to bind type {_type}");
        }
    }
}
