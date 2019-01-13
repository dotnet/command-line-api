using System.Collections.Generic;
using System.Reflection;

namespace System.CommandLine.JackFruit
{
    internal class ChildStrategies
    {
        internal static (bool, IEnumerable<ISymbolBase>) FromMethod(Command command, MethodInfo methodInfo)
        {
            throw new NotImplementedException();
        }

        internal static (bool, IEnumerable<ISymbolBase>) FromType(Command command, Type type)
        {
            throw new NotImplementedException();
        }
    }
}
