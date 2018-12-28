using System;
using System.Collections.Generic;
using System.CommandLine.JackFruit;
using System.Reflection;

namespace System.CommandLine.JackFruit.Tests
{
    internal class HybridModelDescriptionFinder : IDescriptionFinder
    {
        private Dictionary<Type, string> commandHelp = new Dictionary<Type, string>()
        {
            [typeof(DotnetHybrid.Tool)] = "Install or manage tools that extend the .NET experience.",
            [typeof(DotnetHybrid.Add)] = "Add a package or reference to a .NET project.",
            [typeof(DotnetHybrid.Remove)] = "Remove a package or reference from a .NET project.",
            [typeof(DotnetHybrid.List)] = "List project references of a .NET project.",
        };

        private const string verbosityHelp = "Set the MSBuild verbosity level.Allowed values are q[uiet], m[inimal], n[ormal], d[etailed], and diag[nostic]";
        private const string configFileHelp = "The NuGet configuration file to use.";
        private const string projectFileHelp = "The project file to operate on. If a file is not specified, the command will search the current directory for one.";
        private const string addFrameworkHelp = "Add the reference only when targeting a specific framework.";
        private const string removeFrameworkHelp = "Remove the reference only when targeting a specific framework.";

        //private class SpecialComparer : IEqualityComparer<(Type type, string name)>
        //{
        //    public bool Equals((Type type, string name) x, (Type type, string name ) y)
        //    {
        //        return x.type == y.type && x.name.ToUpperInvariant() == y.name.ToUpperInvariant();
        //    }

        //    public int GetHashCode((Type type, string name) obj)
        //    {
        //        return obj.type.GetHashCode() ^ obj.name.ToUpperInvariant().GetHashCode();
        //    }
        //}

        private Dictionary<(Type, string), string> symbolHelp = new Dictionary<(Type, string), string>()
        {
            [(typeof(DotnetHybrid.Tool), nameof(DotnetHybrid.Tool.Install).ToKebabCase())] = "Install a tool for use on the command line.",
            [(typeof(DotnetHybrid.Tool), nameof(DotnetHybrid.Tool.Uninstall))] = "Uninstall a tool from the current development environment.",
            [(typeof(DotnetHybrid.Tool), nameof(DotnetHybrid.Tool.Update).ToKebabCase())] = "Update a tool to the latest stable version.",
            [(typeof(DotnetHybrid.Tool), nameof(DotnetHybrid.Tool.List).ToKebabCase())] = "List tools installed in the current development environment. ",
            [(typeof(DotnetHybrid.Add), nameof(DotnetHybrid.Add.Package).ToKebabCase())] = "Add a NuGet package reference to the project.",
            [(typeof(DotnetHybrid.Add), nameof(DotnetHybrid.Add.Reference).ToKebabCase())] = "Add a project-to-project reference to the project.",
            [(typeof(DotnetHybrid.Remove), nameof(DotnetHybrid.Add.Package).ToKebabCase())] = "Remove a NuGet package reference from the project.",
            [(typeof(DotnetHybrid.Remove), nameof(DotnetHybrid.Remove.Reference).ToKebabCase())] = "Remove a project-to-project reference from the project.",
            [(typeof(DotnetHybrid.List), nameof(DotnetHybrid.List.Package).ToKebabCase())] = "List all NuGet packages for the project.",
            [(typeof(DotnetHybrid.List), nameof(DotnetHybrid.List.Reference).ToKebabCase())] = "List all project-to-project references of the project.",
            [(typeof(DotnetHybrid.Add), nameof(DotnetHybrid.Add.ProjectFile).ToKebabCase())] = projectFileHelp,
            [(typeof(DotnetHybrid.Remove), nameof(DotnetHybrid.Remove.ProjectFileArg).ToKebabCase())] = projectFileHelp,
            [(typeof(DotnetHybrid.List), nameof(DotnetHybrid.List.ProjectFile).ToKebabCase())] = projectFileHelp,
            [(typeof(DotnetHybrid.Tool), (nameof(DotnetHybrid.Tool.Install) + ".PackageId").ToKebabCase())] = "The NuGet Package Id of the tool to install.",
            [(typeof(DotnetHybrid.Tool), (nameof(DotnetHybrid.Tool.Install) + ".Global").ToKebabCase())] = "Install a tool for use on the command line.",
            [(typeof(DotnetHybrid.Tool), (nameof(DotnetHybrid.Tool.Install) + ".ToolPath").ToKebabCase())] = "The directory where the tool will be installed.The directory will be created if it does not exist.",
            [(typeof(DotnetHybrid.Tool), (nameof(DotnetHybrid.Tool.Install) + ".Version").ToKebabCase())] = "The version of the tool package to install.",
            [(typeof(DotnetHybrid.Tool), (nameof(DotnetHybrid.Tool.Install) + ".ConfigFile").ToKebabCase())] = configFileHelp,
            [(typeof(DotnetHybrid.Tool), (nameof(DotnetHybrid.Tool.Install) + ".AddSource").ToKebabCase())] = "Add an additional NuGet package source to use during installation.",
            [(typeof(DotnetHybrid.Tool), (nameof(DotnetHybrid.Tool.Install) + ".Framework").ToKebabCase())] = "The target framework to install the tool for.",
            [(typeof(DotnetHybrid.Tool), (nameof(DotnetHybrid.Tool.Install) + ".Verbosity").ToKebabCase())] = verbosityHelp,
            [(typeof(DotnetHybrid.Tool), (nameof(DotnetHybrid.Tool.Update) + ".PackageId").ToKebabCase())] = "The NuGet Package Id of the tool to install.",
            [(typeof(DotnetHybrid.Tool), (nameof(DotnetHybrid.Tool.Update) + ".Global").ToKebabCase())] = "Update the tool in the current user's tools directory.",
            [(typeof(DotnetHybrid.Tool), (nameof(DotnetHybrid.Tool.Update) + ".ToolPath").ToKebabCase())] = "The directory containing the tool to update.",
            [(typeof(DotnetHybrid.Tool), (nameof(DotnetHybrid.Tool.Update) + ".ConfigFile").ToKebabCase())] = configFileHelp,
            [(typeof(DotnetHybrid.Tool), (nameof(DotnetHybrid.Tool.Update) + ".AddSource").ToKebabCase())] = "Add an additional NuGet package source to use during update.",
            [(typeof(DotnetHybrid.Tool), (nameof(DotnetHybrid.Tool.Update) + ".Framework").ToKebabCase())] = "The target framework to update the tool for.",
            [(typeof(DotnetHybrid.Tool), (nameof(DotnetHybrid.Tool.Update) + ".Verbosity").ToKebabCase())] = verbosityHelp,
            [(typeof(DotnetHybrid.Tool), (nameof(DotnetHybrid.Tool.Uninstall) + ".PackageId").ToKebabCase())] = "The NuGet Package Id of the tool to install.",
            [(typeof(DotnetHybrid.Tool), (nameof(DotnetHybrid.Tool.Uninstall) + ".Global").ToKebabCase())] = "Uninstall the tool from the current user's tools directory.",
            [(typeof(DotnetHybrid.Tool), (nameof(DotnetHybrid.Tool.Uninstall) + ".ToolPath").ToKebabCase())] = "The directory containing the tool to uninstall.",
            [(typeof(DotnetHybrid.Tool), (nameof(DotnetHybrid.Tool.List) + ".Global").ToKebabCase())] = "List tools in the current user's tools directory.",
            [(typeof(DotnetHybrid.Tool), (nameof(DotnetHybrid.Tool.List) + ".ToolPath").ToKebabCase())] = "The directory containing the tools to list.",
            [(typeof(DotnetHybrid.Add), (nameof(DotnetHybrid.Add.Package) + ".PackageName").ToKebabCase())] = "The package reference to add.",
            [(typeof(DotnetHybrid.Add), (nameof(DotnetHybrid.Add.Package) + ".Framework").ToKebabCase())] = addFrameworkHelp,
            [(typeof(DotnetHybrid.Add), (nameof(DotnetHybrid.Add.Package) + ".Source").ToKebabCase())] = "The NuGet package source to use during the restore.",
            [(typeof(DotnetHybrid.Add), (nameof(DotnetHybrid.Add.Package) + ".NoRestore").ToKebabCase())] = "Add the reference without performing restore preview and compatibility check.",
            [(typeof(DotnetHybrid.Add), (nameof(DotnetHybrid.Add.Package) + ".Interactive").ToKebabCase())] = addFrameworkHelp,
            [(typeof(DotnetHybrid.Add), (nameof(DotnetHybrid.Add.Package) + ".PackageDirectory").ToKebabCase())] = "The directory to restore packages to.",
            [(typeof(DotnetHybrid.Add), (nameof(DotnetHybrid.Add.Reference) + ".ProjectPath").ToKebabCase())] = "The paths to the projects to add as references.",
            [(typeof(DotnetHybrid.Add), (nameof(DotnetHybrid.Add.Reference) + ".Framework").ToKebabCase())] = addFrameworkHelp,
            [(typeof(DotnetHybrid.Remove), (nameof(DotnetHybrid.Remove.Package) + ".PackageName").ToKebabCase())] = "The package reference to remove.",
            [(typeof(DotnetHybrid.Remove), (nameof(DotnetHybrid.Remove.Reference) + ".ProjectPath").ToKebabCase())] = "The paths to the referenced projects to remove.",
            [(typeof(DotnetHybrid.Remove), (nameof(DotnetHybrid.Remove.Reference) + ".Framework").ToKebabCase())] = removeFrameworkHelp,
        };

        public string Description<TSource>(TSource source)
        {
            string name = null;
            Type type = null;
            string help = null;
            switch (source)
            {
                case Type sourceType:
                    commandHelp.TryGetValue(sourceType, out help);
                    return help;
                case MethodInfo methodInfo:
                    type = methodInfo.DeclaringType;
                    name = methodInfo.Name;
                    break;
                case PropertyInfo propertyInfo:
                    type = propertyInfo.DeclaringType;
                    name = propertyInfo.Name;
                    break;
                case ParameterInfo parameterInfo:
                    type = parameterInfo.Member.DeclaringType;
                    name = parameterInfo.Member.Name + "." + parameterInfo.Name;
                    break;
            }
            name = name.Contains("_")
                   ? name.Replace("_","")
                   : name;
            name = name.Contains("-")
                    ? name // already kebab case
                    : name.ToKebabCase();

            symbolHelp.TryGetValue((type, name.ToLowerInvariant()), out help);
            return help;
        }

        public string Description<TSource, TItem>(TSource source, TItem child)
            => source is Type resultType
                && symbolHelp.TryGetValue((resultType, (GetName(child))), out string help)
                    ? help
                    : "";

        private string GetName<TItem>(TItem child)
            => child is PropertyInfo property
                ? property.Name
                : child is ParameterInfo parameter
                    ? parameter.Name
                    : "";
    }
}
