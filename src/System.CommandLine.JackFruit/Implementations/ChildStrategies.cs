using System.Collections.Generic;
using System.Reflection;

namespace System.CommandLine.JackFruit
{
    internal class ChildStrategies
    {
        internal static (bool, IEnumerable<ISymbolBase>) FromMethod(Command command, MethodInfo methodInfo)
        {
            //var parameterInfos = methodInfo.GetParameters();
            //var options = filtered.Select(p => GetOption(parent, p));
            //return (false, options);
            throw new NotImplementedException();
        }

        internal static (bool, IEnumerable<ISymbolBase>) FromType(Command command, Type type)
        {
            throw new NotImplementedException();
        }
    }
}
