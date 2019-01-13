using System.CommandLine.Invocation;
using System.Linq;
using System.Reflection;

namespace System.CommandLine.JackFruit
{
    public static class HandlerStrategies
    {
        public static  ICommandHandler FromMethod(Command parent, MethodInfo methodInfo)
        {
            return  ReflectionCommandHandler.Create(methodInfo);
        }

        public static  ICommandHandler FromType(Command parent, Type type)
        {
            return  ReflectionCommandHandler.Create(type);
        }

        private static MethodInfo FindInvokeMethod(Type type)
        {
            var methods = type.GetMethods()
                .Where(x => !x.IsSpecialName && x.DeclaringType != typeof(object));
            if (methods.Count() <= 1)
            {
                return methods.FirstOrDefault();
            }
            var invokeMethods = methods
                            .Where(x => x.Name.StartsWith("Invoke"));
            switch (invokeMethods.Count())
            {
                case 0:
                    // there are methods, but none contain "Invoke"
                    throw new InvalidOperationException("Cannot determine Invoke method");
                case 1:
                    return invokeMethods.First();
                default:
                    var method = invokeMethods.Where(x => x.Name == "InvokeAsync").FirstOrDefault();
                    if (method != null)
                    {
                        return method;
                    }
                    method = invokeMethods.Where(x => x.Name == "Invoke").FirstOrDefault();
                    if (method != null)
                    {
                        return method;
                    }
                    throw new InvalidOperationException("Cannot decide between ambiguous Invoke methods");
            }
        }

    }
}
