using System.CommandLine.Invocation;
using System.Reflection;

namespace System.CommandLine.JackFruit
{
    public class HandlerFinder : FinderBase<HandlerFinder, ICommandHandler>
    {
        private static (bool, ICommandHandler) FromMethod(Command parent, MethodInfo method)
            => method != null
                  ? (false, CommandHandler.Create(method))
                  : (false, null);

        private static (bool, ICommandHandler) FromInvokeOnType(Command parent, Type type)
        {
            var invokeMethod = type.GetMethod("InvokeAsync");
            return type != null && invokeMethod != null
                             ? (false, CommandHandler.Create(invokeMethod))
                             : (false, null);
        }

        public static HandlerFinder Default() 
            => new HandlerFinder()
                    .AddApproachFromFunc<MethodInfo>(FromMethod)
                    .AddApproachFromFunc<Type>(FromInvokeOnType);
    }
}
