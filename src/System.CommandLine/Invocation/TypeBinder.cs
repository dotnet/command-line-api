using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace System.CommandLine.Invocation
{
    public class TypeBinder
    {
        private readonly Type _type;

        public TypeBinder(Type type)
        {
            _type = type ?? throw new ArgumentNullException(nameof(type));
        }

        public object CreateInstance(InvocationContext context)
        {
            var ctor = GetConstructor();

            object[] constructorArgs = Binder.GetMethodArguments(
                context,
                ctor.GetParameters());

            object instance = ctor.Invoke(constructorArgs);

            Binder.SetProperties(context, instance);

            return instance;
        }

        public IEnumerable<Option> BuildOptions()
        {
            var optionSet = new SymbolSet();

            foreach (var parameter in GetConstructor().GetParameters())
            {
                optionSet.Add(parameter.BuildOption());
            }

            foreach (var property in _type.GetProperties()
                                          .Where(p => p.CanWrite))
            {
                var option = property.BuildOption();

                if (!optionSet.Contains(option.Name))
                {
                    optionSet.Add(option);
                }
            }

            return optionSet.Cast<Option>();
        }

        protected virtual ConstructorInfo GetConstructor()
        {
            // TODO: Clean up to consider multiple constructors

            return _type.GetConstructors().SingleOrDefault() ??
                   throw new ArgumentException($"No eligible constructor found to bind type {_type}");
        }
    }
}
