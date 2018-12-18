using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace System.CommandLine.JackFruit
{
    public abstract class TypeCommandBinder : CommandBinder<Type, PropertyInfo>
    {
        private Stack<string> parentNames;

        public TypeCommandBinder(
                  IDescriptionProvider<Type> descriptionProvider = null,
                  IHelpProvider<Type, PropertyInfo> helpProvider = null,
                  IOptionBinder<Type, PropertyInfo> optionProvider = null,
                  IArgumentBinder<Type, PropertyInfo> argumentProvider = null,
                  IInvocationProvider invocationProvider = null)
            : base(descriptionProvider,
                   helpProvider ?? new TypeHelpProvider(descriptionProvider),
                   optionProvider ?? new PropertyInfoOptionBinder(),
                   argumentProvider ?? new TypeArgumentBinder(),
                   invocationProvider)
        {
            parentNames = new Stack<string>();
        }

        public override Command GetCommand(Type currentType)
        {
            var name = GetName(currentType);
            parentNames.Push(name);
            var command = FillCommand(currentType, new Command(name: name.ToKebabCase()));
            parentNames.Pop();
            return command;
        }

        public override RootCommand GetRootCommand(Type currentType)
            => FillCommand(currentType, new RootCommand());

        public string GetName(Type currentType)
        {
            var candidate = currentType.Name;
            var reverseNames = parentNames.Reverse();
            foreach (var name in reverseNames)
            {
                if (candidate.StartsWith(name))
                {
                    candidate = candidate.Substring(name.Length);
                }
            }
            return candidate;
        }

        public override string GetHelp(Type currentType)
        {
            var attribute = currentType.GetCustomAttribute<HelpAttribute>(); ;
            return Extensions.GetHelp(attribute, HelpProvider, currentType);
        }

        public override IEnumerable<PropertyInfo> GetOptionSources(Type currentType)
            => currentType.GetProperties();

        protected override void SetHandler(Command command, Type currentType)
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
            var methodInfo = typeof(TypeCommandBinder).GetMethod(nameof(SetHandlerInternal), bindingFlags);
            var constructedMethod = methodInfo.MakeGenericMethod(currentType);
            constructedMethod.Invoke(this, new object[] { command });
        }

        private void SetHandlerInternal<TResult>(Command command)
        {
            Func<TResult, Task<int>> invocation = null;
            if (invocationProvider != null)
            {
                invocation = invocationProvider.InvokeAsyncFunc<TResult>();
            }
            else
            {
                var methodInfo = typeof(TResult).GetMethod("InvokeAsync");
                if (methodInfo != null)
                {
                    invocation = x => (Task<int>)methodInfo.Invoke(x, null);
                }
            }
            if (invocation != null)
            {
                Func<InvocationContext, Task<int>> invocationWrapper
                    = context => InvokeMethodWithResult(context, invocation);
                command.Handler = new SimpleCommandHandler(invocationWrapper);
            }
        }

        private Task<int> InvokeMethodWithResult<TResult>(InvocationContext context, Func<TResult, Task<int>> invocation)
        {
            var result = Activator.CreateInstance<TResult>();
            var binder = new TypeBinder(typeof(TResult));
            binder.SetProperties(context, result);
            return invocation(result);
        }

        private async Task<int> InvokeAsync(InvocationContext x,
            Func<Task<int>> invocation)
        {
            return await invocation();
        }
    }
}
