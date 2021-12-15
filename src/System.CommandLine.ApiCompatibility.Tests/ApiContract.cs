// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace System.CommandLine.ApiCompatibility.Tests;

internal static class ApiContract
{
    public static string GenerateContractForAssembly(Assembly assembly)
    {
        var output = new StringBuilder();
        var types = assembly.GetExportedTypes().OrderBy(t => t.FullName).ToArray();
        var namespaces = types.Select(t => t.Namespace).Distinct().OrderBy(n => n).ToArray();

        var printedMethods = new HashSet<MethodInfo>();

        foreach (var ns in namespaces)
        {
            output.AppendLine(ns);

            foreach (var type in types.Where(t => t.Namespace == ns))
            {
                var isDelegate = typeof(Delegate).IsAssignableFrom(type);

                var typeKind = type.IsValueType
                                   ? type.IsEnum
                                         ? "enum"
                                         : "struct"
                                   : isDelegate
                                       ? "delegate"
                                       : type.IsInterface
                                           ? "interface"
                                           : "class";

                output.Append($"  {type.GetAccessModifiers()} {typeKind} {type.GetReadableTypeName(ns)}");
                
                if (type.BaseType is { } baseType &&
                    baseType != typeof(object))
                {
                    output.Append($" : {baseType.GetReadableTypeName(ns)}");
                }

                if (type.GetInterfaces().OrderBy(i => i.FullName).ToArray() is { Length: > 0 } interfaces)
                {
                    for (var i = 0; i < interfaces.Length; i++)
                    {
                        var @interface = interfaces[i];

                        var delimiter = i == 0 && type.IsInterface
                                            ? " : "
                                            : ", ";

                        output.Append($"{delimiter}{@interface.GetReadableTypeName(ns)}");
                    }
                }

                output.AppendLine();

                if (type.IsEnum)
                {
                    WriteContractForEnum(type, output);
                }
                else
                {
                    WriteContractForClassOrStruct(type, printedMethods, output);
                }
            }
        }

        return output.ToString();
    }

    private static void WriteContractForEnum(
        Type type,
        StringBuilder output)
    {
        var names = Enum.GetNames(type);
        var values = Enum.GetValues(type).Cast<int>().ToArray();

        foreach (var (name, value) in names.Zip(values.Select(v => v.ToString())))
        {
            output.AppendLine($"    {name}={value}");
        }
    }

    private static void WriteContractForClassOrStruct(
        Type type,
        HashSet<MethodInfo> printedMethods,
        StringBuilder output)
    {
        // statics
        // properties
        foreach (var prop in type.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly)
                                 .Where(m => m.DeclaringType == type)
                                 .OrderBy(c => c.Name))
        {
            if (prop.GetMethod?.IsPublic == true)
            {
                if (printedMethods.Add(prop.GetMethod))
                {
                    var setter = prop.GetSetMethod();
                    if (setter is not null)
                    {
                        printedMethods.Add(setter);
                    }

                    output.AppendLine($"    {GetPropertySignature(prop, type.Namespace)}");
                }
            }
        }

        // methods
        foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly)
                                   .Where(m => m.DeclaringType == type &&
                                               !m.IsAssembly &&
                                               !m.IsFamilyAndAssembly &&
                                               !m.IsPrivate)
                                   .OrderBy(m => m.Name)
                                   .ThenBy(m => m.GetParameters().Length))
        {
            if (printedMethods.Add(method))
            {
                output.AppendLine($"    {GetMethodSignature(method, type.Namespace)}");
            }
        }

        // instance
        foreach (var ctor in type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                                 .Where(m => m.DeclaringType == type)
                                 .Where(m => !m.IsAssembly &&
                                             !m.IsFamilyAndAssembly &&
                                             !m.IsPrivate)
                                 .OrderBy(m => m.Name))
        {
            output.AppendLine($"    .ctor({GetParameterSignatures(ctor.GetParameters(), false, type.Namespace)})");
        }

        foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                                 .Where(m => m.DeclaringType == type)
                                 .OrderBy(c => c.Name))
        {
            if (prop.GetMethod is { IsPublic: true, IsAssembly: false })
            {
                if (printedMethods.Add(prop.GetMethod))
                {
                    var setter = prop.GetSetMethod();
                    if (setter is not null)
                    {
                        printedMethods.Add(setter);
                    }

                    output.AppendLine($"    {prop.GetPropertySignature(type.Namespace)}");
                }
            }
        }

        foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                                   .Where(m => m.DeclaringType == type &&
                                               !m.IsAssembly &&
                                               !m.IsFamilyAndAssembly &&
                                               !m.IsPrivate &&
                                               !m.IsPropertyAccessor())
                                   .OrderBy(c => c.Name))
        {
            if (method.Name == "AddAliasInner")
            {
                
            }

            if (printedMethods.Add(method))
            {
                output.AppendLine($"    {GetMethodSignature(method, type.Namespace)}");
            }
        }
    }

    public static string GetPropertySignature(this PropertyInfo property, string omitNamespace)
    {
        var getter = property.GetGetMethod();
        var setter = property.GetSetMethod();

        var (getterVisibility, getterScope) = GetAccessModifiers(getter);
        var (setterVisibility, _) = GetAccessModifiers(setter);

        string overallVisibility = null;

        switch (getterVisibility, setterVisibility)
        {
            case (string g, string s) when g == s:
                overallVisibility = getterVisibility;
                getterVisibility = null;
                setterVisibility = null;
                break;

            case ({ } g, null):
                overallVisibility = g;
                getterVisibility = null;
                break;

            case (null, { } s):
                overallVisibility = s;
                setterVisibility = null;
                break;
        }

        var getterSignature = string.Empty;
        var setterSignature = string.Empty;

        if (getter is { })
        {
            getterSignature = $"{getterVisibility} get; ";
        }

        if (setter is { })
        {
            setterSignature = $"{setterVisibility} set; ";
        }

        return
            $"{overallVisibility} {getterScope} {GetReadableTypeName(property.PropertyType, omitNamespace)} {property.Name} {{ {getterSignature}{setterSignature}}}"
                .Replace("  ", " ");
    }

    public static string GetMethodSignature(
        this MethodInfo method,
        string omitNamespace)
    {
        var (methodVisibility, methodScope) = GetAccessModifiers(method);

        var genericArgs = string.Empty;

        if (method.IsGenericMethod)
        {
            genericArgs = $"<{string.Join(", ", method.GetGenericArguments().Select(a => GetReadableTypeName(a, omitNamespace)))}>";
        }

        var methodParameters = method.GetParameters().AsEnumerable();

        var isExtensionMethod = method.IsDefined(typeof(ExtensionAttribute), false);

        var parameters = GetParameterSignatures(methodParameters, isExtensionMethod, omitNamespace);

        return
            $"{methodVisibility} {methodScope} {GetReadableTypeName(method.ReturnType, omitNamespace)} {method.Name}{genericArgs}({parameters})".Replace("  ", " ");
    }

    public static string GetParameterSignatures(
        this IEnumerable<ParameterInfo> parameters,
        bool isExtensionMethod,
        string omitNamespace)
    {
        var signature = parameters.Select(param =>
        {
            var signature = string.Empty;

            if (param.ParameterType.IsByRef)
            {
                signature = "ref ";
            }
            else if (param.IsOut)
            {
                signature = "out ";
            }
            else if (isExtensionMethod && param.Position == 0)
            {
                signature = "this ";
            }

            signature += $"{GetReadableTypeName(param.ParameterType, omitNamespace)} {param.Name}";

            if (param.HasDefaultValue)
            {
                signature += $" = {param.DefaultValue ?? "null"}";
            }

            return signature;
        });

        return string.Join(", ", signature);
    }

    private static (string visibility, string scope) GetAccessModifiers(this MethodBase method)
    {
        string visibility = null;
        string scope = null;

        if (method is null)
        {
            return (null, null);
        }

        if (method.IsAssembly)
        {
            visibility = "internal";

            if (method.IsFamily)
            {
                visibility += " protected";
            }
        }
        else if (method.IsPublic)
        {
            visibility = "public";
        }
        else if (method.IsPrivate)
        {
            visibility = "private";
        }
        else if (method.IsFamily)
        {
            visibility = "protected";
        }

        if (method.IsStatic)
        {
            scope = "static";
        }

        return (visibility, scope);
    }

    private static string GetAccessModifiers(this Type type)
    {
        var modifier = string.Empty;

        if (type.IsPublic)
        {
            modifier = "public";
        }

        if (type.IsAbstract && !type.IsInterface)
        {
            if (type.IsSealed)
            {
                modifier += " static";
            }
            else
            {
                modifier += " abstract";
            }
        }

        return modifier;
    }

    public static bool IsPropertyAccessor(this MethodInfo methodInfo) =>
        methodInfo.DeclaringType.GetProperties().Any(prop => prop.GetSetMethod() == methodInfo);

    public static string GetReadableTypeName(this Type type, string omitNamespace)
    {
        var builder = new StringBuilder();
        using var writer = new StringWriter(builder);
        WriteCSharpDeclarationTo(type, writer, omitNamespace);
        writer.Flush();
        return builder.ToString();
    }

    private static void WriteCSharpDeclarationTo(
        this Type type,
        TextWriter writer,
        string omitNamespace)
    {
        var typeName = type.Namespace == omitNamespace
                           ? type.Name
                           : type.FullName ?? type.Name;

        if (typeName.Contains("`"))
        {
            writer.Write(typeName.Remove(typeName.IndexOf('`')));
            writer.Write("<");
            var genericArguments = type.GetGenericArguments();

            for (var i = 0; i < genericArguments.Length; i++)
            {
                var genericArg = genericArguments[i];

                if (genericArg.IsGenericParameter)
                {
                    if (genericArg.GenericParameterAttributes.HasFlag(GenericParameterAttributes.Covariant))
                    {
                        writer.Write("out ");
                    }
                    else if (genericArg.GenericParameterAttributes.HasFlag(GenericParameterAttributes.Contravariant))
                    {
                        writer.Write("in ");
                    }
                }

                WriteCSharpDeclarationTo(genericArg, writer, omitNamespace);
                if (i < genericArguments.Length - 1)
                {
                    writer.Write(",");
                }
            }

            writer.Write(">");
        }
        else
        {
            writer.Write(typeName);
        }
    }
}