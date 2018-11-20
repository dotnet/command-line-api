using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace System.CommandLine.JackFruit
{
    public abstract class FutureTypeCommandBinder : CommandBinder<Type, PropertyInfo>
    {
        public FutureTypeCommandBinder(
                    IDescriptionProvider<Type> descriptionProvider = null,
                    IHelpProvider<Type, PropertyInfo> helpProvider = null,
                    IOptionBinder<Type, PropertyInfo> optionProvider = null,
                    IArgumentBinder<Type, PropertyInfo> argumentProvider = null,
                    IInvocationProvider invocationProvider = null)
             : base(descriptionProvider, helpProvider, optionProvider,
                   argumentProvider, invocationProvider)
        {
            this.helpProvider = helpProvider
                        ?? new TypeHelpProvider(descriptionProvider);
            this.optionProvider = optionProvider
                                ?? new PropertyInfoOptionBinder();
            this.argumentProvider = argumentProvider
                                ?? new TypeArgumentBinder();
            this.invocationProvider = invocationProvider
                                ?? invocationProvider;

            this.optionProvider.HelpProvider = this.optionProvider.HelpProvider
                                ?? this.helpProvider;
            this.argumentProvider.HelpProvider = this.argumentProvider.HelpProvider
                                ?? this.helpProvider;
        }

        public override string GetHelp(Type currentType)
        {
            var attribute = currentType.GetCustomAttribute<HelpAttribute>(); ;
            return Extensions.GetHelp(attribute, HelpProvider, currentType);
        }

        public override string GetName(Type currentType)
            => currentType.Name;

        public override IEnumerable<PropertyInfo> GetOptionSources(Type currentType)
        {
            return currentType.GetProperties()
                .Where(p => argumentProvider.IsArgument(currentType, p));
        }

        // GetCommandSources must be implemented in the derived class for hierarchical or composition models
    }

    public abstract class CommandBinder<TCommandSource, TOptionSource> : ICommandBinder<TCommandSource>
    {
        private protected IHelpProvider<TCommandSource, TOptionSource> helpProvider;
        private protected IOptionBinder<TCommandSource, TOptionSource> optionProvider;
        private protected IArgumentBinder<TCommandSource, TOptionSource> argumentProvider;
        private protected IInvocationProvider invocationProvider;

        public abstract string GetHelp(TCommandSource source);
        public abstract string GetName(TCommandSource source);
        public abstract IEnumerable<TOptionSource> GetOptionSources(TCommandSource source);
        public abstract IEnumerable<TCommandSource> GetSubCommandSources(TCommandSource source);

        protected CommandBinder(
                    IDescriptionProvider<TCommandSource> descriptionProvider,
                    IHelpProvider<TCommandSource, TOptionSource> helpProvider,
                    IOptionBinder<TCommandSource, TOptionSource> optionProvider,
                    IArgumentBinder<TCommandSource, TOptionSource> argumentProvider,
                    IInvocationProvider invocationProvider = null)
        {
            this.optionProvider.HelpProvider = this.optionProvider.HelpProvider
                                ?? this.helpProvider;
            this.argumentProvider.HelpProvider = this.argumentProvider.HelpProvider
                                ?? this.helpProvider;
        }

        public IHelpProvider<TCommandSource> HelpProvider { get; set; }

        public Argument GetArgument(TCommandSource source)
        {
            return argumentProvider.GetArgument(source);
        }

        public Command GetCommand(TCommandSource source)
            => FillCommand(source, new Command(name: GetName(source)));

        public RootCommand GetRootCommand(TCommandSource source)
            => FillCommand(source, new RootCommand());

        private TCommand FillCommand<TCommand>(TCommandSource source, TCommand command)
            where TCommand : Command
        {
            command.Description = GetHelp(source);
            SetHandler(command, source);

            command.AddOptions(GetOptions(source));
            command.Argument = GetArgument(source);

            return command
                .AddCommands(GetSubCommands(source));
        }

        public IEnumerable<Option> GetOptions(TCommandSource source)
        {
            return GetOptionSources(source)
                .Select(x => optionProvider.GetOption(source, x));
        }

        public IEnumerable<Command> GetSubCommands(TCommandSource source)
        {
            var subCommandTypes = GetSubCommandSources(source);
            return subCommandTypes == null
                ? null
                : subCommandTypes
                    .Select(t => GetCommand(t));
        }


        private void SetHandler(Command command, TCommandSource source)
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
            var methodInfo = typeof(TypeCommandBinder).GetMethod(nameof(SetHandlerInternal), bindingFlags);
            var constructedMethod = methodInfo.MakeGenericMethod(typeof(TCommandSource));
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
