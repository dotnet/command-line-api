using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine.Invocation
{
    // This should be named Binder, but that name was taken at present
    public class BindingSet 
    {
        private List<Binding> _bindings { get; } = new List<Binding>();

        public void AddBinding(BindingSide targetSide, BindingSide parserSide)
            => _bindings.Add(new Binding(targetSide, parserSide));

        public void AddBinding(Binding binding)
            => _bindings.Add(binding);

        public void BindDefaults(InvocationContext context, object target)
        {
            foreach (var binding in _bindings)
            {
                binding.BindDefaults(context, target);
            }
        }

        public void Bind(InvocationContext context, object target)
        {
            foreach (var binding in _bindings)
            {
                binding.Bind(context, target);
            }
        }

        // To array forces copy so no changes can be made
        public IEnumerable<Binding> Bindings
            => _bindings.ToArray();

        public IEnumerable<Binding> FindTargetBinding<T>(Func<T, bool> predicate)
            where T : BindingSide 
            => _bindings
                     .Where(b => b.TargetSide is T bs && predicate(bs));
    }
}
