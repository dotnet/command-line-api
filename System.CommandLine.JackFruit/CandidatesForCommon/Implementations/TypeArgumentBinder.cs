using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace System.CommandLine.JackFruit
{
    public class TypeArgumentBinder : IArgumentBinder<Type, PropertyInfo>
    {
        // TODO: Extract into IEnumerable(ArgumentStrategy) with IArgumentStrategy having 
        // CanHandle, GetArgument, GetName, GetHelp. GetArgument(Type, Name, Description)
        // And figure out rules, etc. 
        public IHelpProvider<Type> HelpProvider { get; set; }

        public Argument GetArgument(Type source)
        {
            var propertyInfo = GetArgumentPropertyInfo(source);
            if (propertyInfo != null)
            {
                return GetArgument(source, propertyInfo);
            }
            var parameterInfo = GetArgumentParameterInfo(source);
            if (parameterInfo != null)
            {
                return GetArgument(source, parameterInfo);
            }
            return null;
        }

        public string GetHelp(Type source)
        {
            var propertyInfo = GetArgumentPropertyInfo(source);
            if (propertyInfo != null)
            {
                ;
                return Extensions.GetHelp(
                    propertyInfo.GetCustomAttribute<HelpAttribute>(), 
                    HelpProvider, source, propertyInfo.Name);
            }
            var parameterInfo = GetArgumentParameterInfo(source);
            if (parameterInfo == null)
            {
                return "";
            }
;
            return Extensions.GetHelp(
                parameterInfo.GetCustomAttribute<HelpAttribute>(), 
                HelpProvider, source, parameterInfo.Name);
        }

        public string GetName(Type source)
        {
            var propertyInfo = GetArgumentPropertyInfo(source);
            if (propertyInfo != null)
            {
                return propertyInfo.Name;
            }
            return GetArgumentParameterInfo(source)?.Name;
        }

        public bool IsArgument(Type source, PropertyInfo propertyInfo)
            => !(IsPropertyAnArgument(propertyInfo) || propertyInfo.IgnoreProperty());

        private PropertyInfo GetArgumentPropertyInfo(Type source) 
            => source.GetProperties()
                        .Where(prop => IsArgument(source, prop))
                        .FirstOrDefault();


        private ParameterInfo GetArgumentParameterInfo(Type source)
        {
            var ctors = source.GetConstructors()
                            .Where(c => c.GetParameters().Count() == 1);
            if (ctors.Count() == 0)
            {
                return null;
            }
            if (ctors.Count() > 1)
            {
                throw new InvalidOperationException("Multiple constructors with a single parameter cannot be resolved");
            }
            return ctors.First().GetParameters().First();
        }


        public bool IsPropertyAnArgument(PropertyInfo propertyInfo)
          => propertyInfo.Name.EndsWith("Arg") ||
                 propertyInfo
                     .CustomAttributes
                     .Any(x => x.AttributeType == typeof(ArgumentAttribute));

        private Argument GetArgument(Type currentType, PropertyInfo property)
        {
            var argument = new Argument
            {
                ArgumentType = property.PropertyType
            };
            argument.Name = property.Name.EndsWith("Arg")
                        ? property.Name.Substring(0, property.Name.Length - 3)
                        : property.Name;
            argument.Description = HelpProvider.GetHelp(currentType, property.Name);
            return argument;
        }

        private Argument GetArgument(Type currentType, ParameterInfo parameter)
        {
            var argument = new Argument
            {
                ArgumentType = parameter.ParameterType
            };
            argument.Name = parameter.Name;
            argument.Description = HelpProvider.GetHelp(currentType, parameter.Name);
            return argument;
        }

    }
}
