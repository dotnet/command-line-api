using System.CommandLine.Invocation;
using System.Linq;
using System.Reflection;

namespace System.CommandLine.JackFruit
{
    public class ParameterInfoOptionBinder : IOptionBinder<MethodInfo, ParameterInfo>
    {
        public IHelpProvider<MethodInfo> HelpProvider { get; set; }

        public string GetHelp(MethodInfo parent, ParameterInfo parameterInfo)
            => HelpProvider?.GetHelp(parent, parameterInfo);

        public string GetName(MethodInfo parent, ParameterInfo parameterInfo)
            => parameterInfo.Name;

        public Option GetOption(MethodInfo parent, ParameterInfo parameterInfo)
        {
            var option = Invocation.Binder.BuildOption(parameterInfo);
            option.Description = GetHelp(parent, parameterInfo);
            var aliasAttribute = parameterInfo.GetCustomAttribute<AliasAttribute>();
            if (aliasAttribute != null)
            {
                foreach (var alias in aliasAttribute.Aliases)
                {
                    option.AddAlias(alias);
                }
            }
            else
            {
                option.AddAliases( Extensions.AliasesFromUnderscores(option.Name));
                option.Name = option.Aliases.First();
            }
            return option;
        }

    }
}
