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

    public abstract class ReflectionCommandHandler : ICommandHandler
    {

        public static ReflectionCommandHandler Create(MethodInfo methodInfo)
        {
            var handler = CreateInternal(methodInfo.DeclaringType, methodInfo, null);
            return handler;
        }

        public static ReflectionCommandHandler Create(Type declaringType)
        {
            var handler = CreateInternal(declaringType, null, null);
            return handler;
        }

        public static ReflectionCommandHandler Create(MethodInfo methodInfo, object target)
        {
            var handler = CreateInternal(methodInfo.DeclaringType, methodInfo, target);
            return handler;
        }

        private static ReflectionCommandHandler CreateInternal(Type declaringType, MethodInfo methodInfo, object target)
        {
            var genericMethod = typeof(ReflectionCommandHandler).GetMethods()
                                    .Where(x => x.Name == "Create" && x.IsGenericMethod
                                            && x.GetParameters().FirstOrDefault()?.ParameterType == typeof(MethodInfo))
                                    .First();
            var constructedMethod = genericMethod.MakeGenericMethod(declaringType);
            var handler = constructedMethod.Invoke(null, new object[] { methodInfo, target });
            return (ReflectionCommandHandler)handler;
        }

        public static ReflectionCommandHandler<TTarget> Create<TTarget>(MethodInfo methodInfo = null, TTarget target = null)
                where TTarget : class
        {
            var handler = Activator.CreateInstance<ReflectionCommandHandler<TTarget>>();
            handler.Binder.Target = target;
            methodInfo = methodInfo ?? GetInvokeMethod(typeof(TTarget));
            handler.InvocationMethodInfo = methodInfo;
            return handler;
        }

        private static MethodInfo GetInvokeMethod(Type type)
        {
            var methodInfo = type.GetMethod("InvokeAsync");
            return methodInfo ?? type.GetMethods()
                                     .Where(x => x.Name.StartsWith("Invoke"))
                                     .FirstOrDefault();
        }

        public abstract Task<int> InvokeAsync(InvocationContext context);

        protected MethodInfo InvocationMethodInfo { get; set; }
    }

    public class ReflectionCommandHandler<TTarget> : ReflectionCommandHandler
        where TTarget : class
    {
        public ReflectionBinder<TTarget> Binder { get; set; } = new ReflectionBinder<TTarget>();

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        // TODO: How do we await this via reflection?
        public override Task<int> InvokeAsync(InvocationContext context)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var target = Binder.CreateInstance(context);
            var methodArguments = Binder.GetParameterValues(context,target, InvocationMethodInfo);

            var value = InvocationMethodInfo.Invoke(target, methodArguments);

            return CommandHandler.GetResultCodeAsync(value, context);
        }

 }
}
