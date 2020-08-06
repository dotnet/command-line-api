// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace System.CommandLine.DragonFruit
{
    public class EntryPointDiscoverer
    {
        public static MethodInfo FindStaticEntryMethod(Assembly assembly, string entryPointFullTypeName = null)
        {
            var candidates = new List<MethodInfo>();

            if (!string.IsNullOrWhiteSpace(entryPointFullTypeName))
            {
                var typeInfo = assembly.GetType(entryPointFullTypeName, false, false)?.GetTypeInfo();
                if (typeInfo == null)
                {
                    throw new InvalidProgramException($"Could not find '{entryPointFullTypeName}' specified for Main method. See <StartupObject> project property.");
                }
                FindMainMethodCandidates(typeInfo, candidates);
            }
            else
            {
                foreach (var type in assembly
                    .DefinedTypes
                    .Where(t => t.IsClass)
                    .Where(t => !t.IsDefined(typeof(CompilerGeneratedAttribute))))
                {
                    FindMainMethodCandidates(type, candidates);
                }
            }

            string MainMethodFullName()
            {
                return string.IsNullOrWhiteSpace(entryPointFullTypeName) ? "Main" : $"{entryPointFullTypeName}.Main";
            }

            if (candidates.Count > 1)
            {
                throw new AmbiguousMatchException(
                    $"Ambiguous entry point. Found multiple static functions named '{MainMethodFullName()}'. Could not identify which method is the main entry point for this function.");
            }

            if (candidates.Count == 0)
            {
                throw new InvalidProgramException(
                    $"Could not find a static entry point '{MainMethodFullName()}' that accepts option parameters.");
            }

            return candidates[0];
        }

        private static void FindMainMethodCandidates(TypeInfo type, List<MethodInfo> candidates)
        {
            foreach (var method in type
                .GetMethods(BindingFlags.Static |
                            BindingFlags.Public |
                            BindingFlags.NonPublic)
                .Where(m =>
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
    }
}
