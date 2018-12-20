using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace System.CommandLine.JackFruit
{
    public class MethodCommandBinder
        : CommandBinder<MethodCommandBinder, MethodInfo, ParameterInfo>
    {
        public MethodCommandBinder(
                  IDescriptionProvider<MethodInfo> descriptionProvider = null,
                  IHelpProvider<MethodInfo> helpProvider = null,
                  IOptionBinder<MethodInfo, ParameterInfo> optionProvider = null,
                  IArgumentBinder<MethodInfo, ParameterInfo> argumentProvider = null,
                  IInvocationProvider invocationProvider = null,
                    bool shouldRemoveParentNames = false)
            : base(descriptionProvider,
                   helpProvider,
                   optionProvider ?? new ParameterInfoOptionBinder(),
                   argumentProvider ?? new MethodInfoArgumentBinder(),
                   invocationProvider,
                   shouldRemoveParentNames)
        { }

        public override string GetName(MethodInfo current) 
            => current.Name;

        public override IEnumerable<ParameterInfo> GetOptionSources(MethodInfo current)
            => current.GetParameters()
                  .Where(p => !argumentProvider.IsArgument(current, p)
                            && !p.IgnoreParameter());

        public override IEnumerable<MethodInfo> GetSubCommandSources(MethodInfo source) 
            => null;

        protected override void SetHandler(Command command, MethodInfo current)
        {
           command.Handler = CommandHandler.Create(current);
        }
   }
}
