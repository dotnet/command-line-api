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

        protected override void SetHandler(Command command, MethodInfo current)
        {
           command.Handler = CommandHandler.Create(current);
        }

        //private void SetHandlerInternal(Command command)
        //{
        //    Func<TResult, Task<int>> invocation = null;
        //    if (invocationProvider != null)
        //    {
        //        invocation = invocationProvider.InvokeAsyncFunc<TResult>();
        //    }
        //    else
        //    {
        //        var methodInfo = typeof(TResult).GetMethod("InvokeAsync");
        //        if (methodInfo != null)
        //        {
        //            invocation = x => (Task<int>)methodInfo.Invoke(x, null);
        //        }
        //    }
        //    if (invocation != null)
        //    {
        //        Func<InvocationContext, Task<int>> invocationWrapper
        //            = context => InvokeMethodWithResult(context, invocation);
        //        command.Handler = new SimpleCommandHandler(invocationWrapper);
        //    }
        //}

        //private async Task<int>  InvokeMethodWithResult<TResult>(
        //    InvocationContext context, Func<TResult, Task<int>> invocation)
        //{
        //}

        //private async Task<int> InvokeAsync(InvocationContext x,
        //    Func<Task<int>> invocation)
        //{
        //    return await invocation();
        //}
    }
}
