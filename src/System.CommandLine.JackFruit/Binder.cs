using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace System.CommandLine.JackFruit
{
    // Scenarios
    // - Appmodel builds binder and parse tree simultaneously
    // - Programmer builds RootCommand, attached invocation
    // - Just have a method and want to auto bind (distinct from App model because should be streamined)
    public class Binder
    {
        private List<BindingBase> BindActions { get; } = new List<BindingBase>();

        public void AddBinding(BindingBase bindingAction) 
            => BindActions.Add(bindingAction);

        public void AddBindings(IEnumerable<BindingBase> bindingActions) 
            => BindActions.AddRange(bindingActions);

        public BindingBase Find(object reflectionObject) 
            => BindActions
                .Where(x => x.ReflectionThing.Equals(reflectionObject))
                .LastOrDefault();

    }
}
