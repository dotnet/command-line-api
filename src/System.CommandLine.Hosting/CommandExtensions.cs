using Microsoft.Extensions.Hosting;
using System.CommandLine.Invocation;
using System.Linq;
using System.Reflection;

namespace System.CommandLine.Hosting
{
    public static class CommandExtensions
    {
        private const string ExecuterMethodName = "Execute";

        /// <summary>
        /// Assign Handler to method of an instance of <typeparamref name="TExecuter"/>
        /// </summary>
        /// <typeparam name="TExecuter">The type of the executer instance</typeparam>
        /// <param name="command">The command to assign the handler to</param>
        /// <param name="methodName">The method name on <typeparamref name="TExecuter"/> that will be called.</param>
        public static void AssignHandler<TExecuter>(this Command command, string methodName = null)
            where TExecuter : class
        {
            if (string.IsNullOrWhiteSpace(methodName))
            {
                methodName = GetMethodName(command);
            }

            command.Handler = CommandHandler.Create(async (IHost host, InvocationContext context) =>
            {
                var executer = host.Services.GetService(typeof(TExecuter)) as TExecuter;
                MethodInfo executeAction = typeof(TExecuter)
                    .GetMethods()
                    .FirstOrDefault(m => m.Name.Equals(methodName, StringComparison.OrdinalIgnoreCase));
                return await CommandHandler.Create(executeAction, executer).InvokeAsync(context);
            });
        }

        private static string GetMethodName(Command command)
        {
            if (command is RootCommand)
            {
                return ExecuterMethodName;
            }
            else
            {
                return command.Name;
            }
        }
    }
}
