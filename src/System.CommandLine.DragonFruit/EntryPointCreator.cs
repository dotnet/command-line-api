using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace System.CommandLine.DragonFruit
{
    internal class EntryPointCreator
    {
        internal static MethodInfo FindStaticEntryMethod(Assembly assembly)
        {
            var candidates = new List<MethodInfo>();
            foreach (TypeInfo type in assembly.DefinedTypes.Where(t =>
                !t.IsAbstract && string.Equals("Program", t.Name, StringComparison.OrdinalIgnoreCase)))
            {
                if (type.GetCustomAttribute<CompilerGeneratedAttribute>() != null)
                {
                    continue;
                }

                foreach (MethodInfo method in type.GetMethods(BindingFlags.Static | BindingFlags.Public |
                                                              BindingFlags.NonPublic).Where(m =>
                    string.Equals("Main", m.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    if (method.ReturnType == typeof(void)
                        || method.ReturnType == typeof(int)
                        || method.ReturnType == typeof(Task)
                        || method.ReturnType == typeof(Task<int>))
                    {
                        candidates.Add(method);
                    }
                }
            }

            if (candidates.Count > 1)
            {
                throw new AmbiguousMatchException(
                    "Ambiguous entry point. Found muliple static functions named 'Program.Main'. Could not identify which method is the main entry point for this function.");
            }

            if (candidates.Count == 0)
            {
                throw new InvalidProgramException(
                    "Could not find a static entry point named 'Main' on a type named 'Program' that accepts option parameters.");
            }

            MethodInfo entryMethod = candidates[0];
            return entryMethod;
        }
    }
}
