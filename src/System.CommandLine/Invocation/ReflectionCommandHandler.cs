using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    // You can get data from at least the following places
    // - unrelated to CommandLine, like an environment variable
    // - somewhere else in the result tree
    // - options and arguments (symbols) on this command

    // You always invoke in a context that is
    // - an operating system environment, including environment variables
    // - a running application and any statics on other classes, icnluding service provider/DI
    // - a type, it's properties, static properties and partent prporties/static properties
    //   - the type was created with a constructor that may have had parameters
    // - a method and it's parameters

    // In the case where we have a method info, everything else is available
    // I am designing for the class containing the method to be special purpose 
    //      and call into the rest of the app, for exammple, demanding one constructor

    public class ReflectionCommandHandler : IBoundCommandHandler
    {
        private ReflectionCommandHandler(Type targetType)
        {
            TargetType = targetType;
            Binder = new ReflectionBinder(TargetType);
        }

        public static ReflectionCommandHandler Create(MethodInfo methodInfo)
        {
            var handler = Create(methodInfo.DeclaringType, methodInfo, null);
            return handler;
        }

        public static ReflectionCommandHandler Create(Type declaringType)
        {
            var handler = Create(declaringType, null, null);
            return handler;
        }

        public static ReflectionCommandHandler Create(MethodInfo methodInfo, object target)
        {
            var handler = Create(methodInfo.DeclaringType, methodInfo, target);
            return handler;
        }

        public static ReflectionCommandHandler Create(Type type, MethodInfo methodInfo, object target = null)
        {
            var handler = new ReflectionCommandHandler(type);
            handler.Binder.SetTarget(target);
            methodInfo = methodInfo ?? GetInvokeMethod(type);
            handler.Binder.SetInvocationMethod(methodInfo);
            return handler;
        }

        public Type TargetType { get; }

        public ReflectionBinder Binder { get; }
        IBinder IBoundCommandHandler.Binder
            => this.Binder;

        public Task<int> InvokeAsync(InvocationContext context)
        {
            // Can we get an easier way to get the handler's owner - which only matters
            // for invocation
            Binder.AddBindingsIfNeeded(context?.ParseResult?.CommandResult?.Command);
            var value = Binder.InvokeAsync(context);
            return CommandHandler.GetResultCodeAsync(value, context);
        }

        private static MethodInfo GetInvokeMethod(Type type)
        {
            var methodInfo = type.GetMethod("InvokeAsync");
            return methodInfo ?? type.GetMethods()
                                     .Where(x => x.Name.StartsWith("Invoke"))
                                     .FirstOrDefault();
        }

    }
}
