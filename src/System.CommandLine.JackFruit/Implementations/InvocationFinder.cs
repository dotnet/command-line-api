using System.CommandLine.Invocation;
using System.Reflection;

namespace System.CommandLine.JackFruit
{
    public class HandlerFinder : FinderBase<HandlerFinder, ICommandHandler>
    {
        private static (bool, ICommandHandler) FromMethod(Command[] parents, MethodInfo methodInfo)
        {
            return GetHandler(methodInfo.DeclaringType, methodInfo);

        }

        private static (bool, ICommandHandler) FromInvokeOnType(Command[] parents, Type type)
        {
            var invokeMethod = type.GetMethod("InvokeAsync");
            return GetHandler(type, invokeMethod);
        }

        private static (bool, ICommandHandler) GetHandler(Type type, MethodInfo methodInfo)
        {
            if(methodInfo == null)
            {
                return (false, null);
            }
            var typeBinder = methodInfo.IsStatic
                                ? null
                                : new TypeBinder(type);
            // TODO: This directly access the constructor of TypeBindingCOmmandHandler, which was not intended
            return type != null && methodInfo != null
                    ? (false, new TypeBindingCommandHandler(methodInfo, typeBinder))
                    : (false, null);
        }

        public static HandlerFinder Default() 
            => new HandlerFinder()
                    .AddApproachFromFunc<MethodInfo>(FromMethod)
                    .AddApproachFromFunc<Type>(FromInvokeOnType);
    }
}
