using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace System.CommandLine.JackFruit
{
    public class SimpleCommandHandler : ICommandHandler
    {
        private readonly Func<Task<int>> noArgInvocation;
        private readonly Func<IConsole, Task<int>> consoleOnlyInvocation;
        private readonly Func<InvocationContext, Task<int>> contextInvocation;


        public SimpleCommandHandler(Func<Task<int>> noArgInvocation)
            => this.noArgInvocation = noArgInvocation;

        public SimpleCommandHandler(Func<IConsole, Task<int>> consoleOnlyInvocation)
            => this.consoleOnlyInvocation = consoleOnlyInvocation;

        public SimpleCommandHandler(Func<InvocationContext, Task<int>> contextInvocation)
            => this.contextInvocation = contextInvocation;

        public async Task<int> InvokeAsync(InvocationContext context)
        {
            if (contextInvocation != null)
            {
                return await contextInvocation(context);
            }
            else if (consoleOnlyInvocation != null)
            {
                return 512;
            }
            else if (noArgInvocation != null)
            {
                return await noArgInvocation();
            }
            return 256;
        }
    }
}
