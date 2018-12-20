using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace System.CommandLine.JackFruit
{
    public class MethodInfoArgumentBinder : IArgumentBinder<MethodInfo, ParameterInfo>
    {
        // TODO: Extract into IEnumerable(ArgumentStrategy) with IArgumentStrategy having 
        // CanHandle, GetArgument, GetName, GetHelp. GetArgument(MethodInfo, Name, Description)
        // And figure out rules, etc. 
        public IHelpProvider<MethodInfo> HelpProvider { get; set; }
        IHelpProvider<MethodInfo> IArgumentBinder<MethodInfo, ParameterInfo>.HelpProvider { get; set; }

        public Argument GetArgument(MethodInfo source)
        {
            var parameterInfo = GetArgumentParameterInfo(source);
            if (parameterInfo != null)
            {
                return GetArgument(source, parameterInfo);
            }
            return null;
        }

        public string GetHelp(MethodInfo source)
            => HelpProvider?.GetHelp(source, GetArgumentParameterInfo(source))
                 ?? HelpProvider?.GetHelp(source, GetArgumentParameterInfo(source));

        public string GetName(MethodInfo source)
        {
            var parameterInfo = GetArgumentParameterInfo(source);
            if (parameterInfo != null)
            {
                return parameterInfo.Name;
            }
            return GetArgumentParameterInfo(source)?.Name;
        }

        public bool IsArgument(MethodInfo source, ParameterInfo parameterInfo)
            => IsParameterAnArgument(parameterInfo) &&  !parameterInfo.IgnoreParameter();

        private ParameterInfo GetArgumentParameterInfo(MethodInfo source) 
            => source.GetParameters()
                        .Where(prop => IsArgument(source, prop))
                        .FirstOrDefault();

        public bool IsParameterAnArgument(ParameterInfo parameterInfo)
          => parameterInfo.Name.EndsWith("Arg") ||
                 parameterInfo
                     .CustomAttributes
                     .Any(x => x.AttributeType == typeof(ArgumentAttribute));

        private Argument GetArgument(MethodInfo currentType, ParameterInfo parameter)
        {
            var argument = new Argument
            {
                ArgumentType = parameter.ParameterType
            };
            argument.Name = parameter.Name.EndsWith("Arg")
                        ? parameter.Name.Substring(0, parameter.Name.Length - 3)
                        : parameter.Name;
            argument.Description = HelpProvider.GetHelp(currentType, parameter);
            return argument;
        }
    }
}
