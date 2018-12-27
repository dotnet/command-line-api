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
        private Dictionary<(Type, string), string> symbolHelp = new Dictionary<(Type, string), string>()
        {
            [(typeof(DotnetHybrid.Tool), nameof(DotnetHybrid.Tool.Install))] = "Install a tool for use on the command line.",
            [(typeof(DotnetHybrid.Tool), nameof(DotnetHybrid.Tool.Uninstall))] = "Uninstall a tool from the current development environment.",
            [(typeof(DotnetHybrid.Tool), nameof(DotnetHybrid.Tool.Update))] = "Update a tool to the latest stable version.",
            [(typeof(DotnetHybrid.Tool), nameof(DotnetHybrid.Tool.List))] = "List tools installed in the current development environment. ",
            [(typeof(DotnetHybrid.Add), nameof(DotnetHybrid.Add.Package))] = "Add a NuGet package reference to the project.",
            [(typeof(DotnetHybrid.Add), nameof(DotnetHybrid.Add.Reference))] = "Add a project-to-project reference to the project.",
            [(typeof(DotnetHybrid.Remove), nameof(DotnetHybrid.Add.Package))] = "Remove a NuGet package reference from the project.",
            [(typeof(DotnetHybrid.Remove), nameof(DotnetHybrid.Remove.Reference))] = "Remove a project-to-project reference from the project.",
            [(typeof(DotnetHybrid.List), nameof(DotnetHybrid.List.Package))] = "List all NuGet packages for the project.",
            [(typeof(DotnetHybrid.List), nameof(DotnetHybrid.List.Reference))] = "List all project-to-project references of the project.",
            [(typeof(DotnetHybrid.Tool), nameof(DotnetHybrid.Tool.Install) + "PackageId")] = "The NuGet Package Id of the tool to install.",
            [(typeof(DotnetHybrid.Tool), nameof(DotnetHybrid.Tool.Install) + "Globa")] = "Install a tool for use on the command line.",
            [(typeof(DotnetHybrid.Tool), nameof(DotnetHybrid.Tool.Install) + "ToolPath")] = "The directory where the tool will be installed.The directory will be created if it does not exist.",
            [(typeof(DotnetHybrid.Tool), nameof(DotnetHybrid.Tool.Install) + "Version")] = "The version of the tool package to install.",
            [(typeof(DotnetHybrid.Tool), nameof(DotnetHybrid.Tool.Install) + "ConfigFile")] = configFileHelp,
            [(typeof(DotnetHybrid.Tool), nameof(DotnetHybrid.Tool.Install) + "AddSource")] = "Add an additional NuGet package source to use during installation.",
            [(typeof(DotnetHybrid.Tool), nameof(DotnetHybrid.Tool.Install) + "Framework")] = "The target framework to install the tool for.",
            [(typeof(DotnetHybrid.Tool), nameof(DotnetHybrid.Tool.Install) + "Verbosity")] = verbosityHelp,
            [(typeof(DotnetHybrid.Tool), nameof(DotnetHybrid.Tool.Update) + "PackageId")] = "The NuGet Package Id of the tool to install.",
            [(typeof(DotnetHybrid.Tool), nameof(DotnetHybrid.Tool.Update) + "Global")] = "Update the tool in the current user's tools directory.",
            [(typeof(DotnetHybrid.Tool), nameof(DotnetHybrid.Tool.Update) + "ToolPath")] = "The directory containing the tool to update.",
            [(typeof(DotnetHybrid.Tool), nameof(DotnetHybrid.Tool.Update) + "ConfigFile")] = configFileHelp,
            [(typeof(DotnetHybrid.Tool), nameof(DotnetHybrid.Tool.Update) + "AddSource")] = "Add an additional NuGet package source to use during update.",
            [(typeof(DotnetHybrid.Tool), nameof(DotnetHybrid.Tool.Update) + "Framework")] = "The target framework to update the tool for.",
            [(typeof(DotnetHybrid.Tool), nameof(DotnetHybrid.Tool.Update) + "Verbosity")] = verbosityHelp,
            [(typeof(DotnetHybrid.Tool), nameof(DotnetHybrid.Tool.Uninstall) + "PackageId")] = "The NuGet Package Id of the tool to install.",
            [(typeof(DotnetHybrid.Tool), nameof(DotnetHybrid.Tool.Uninstall) + "Global")] = "Uninstall the tool from the current user's tools directory.",
            [(typeof(DotnetHybrid.Tool), nameof(DotnetHybrid.Tool.Uninstall) + "ToolPath")] = "The directory containing the tool to uninstall.",
            [(typeof(DotnetHybrid.Tool), nameof(DotnetHybrid.Tool.List) + "Global")] = "List tools in the current user's tools directory.",
            [(typeof(DotnetHybrid.Tool), nameof(DotnetHybrid.Tool.List) + "ToolPath")] = "The directory containing the tools to list.",
            [(typeof(DotnetHybrid.Add), nameof(DotnetHybrid.Add) + "ProjectFile")] = projectFileHelp,
            [(typeof(DotnetHybrid.Remove), nameof(DotnetHybrid.Remove) + "ProjectFileArg")] = projectFileHelp,
            [(typeof(DotnetHybrid.List), nameof(DotnetHybrid.List) + "ProjectFile")] = projectFileHelp,
            [(typeof(DotnetHybrid.Add), nameof(DotnetHybrid.Add) + "Package.PackageName")] = "The package reference to add.",
            [(typeof(DotnetHybrid.Add), nameof(DotnetHybrid.Add) + "Package._Framework")] = addFrameworkHelp,
            [(typeof(DotnetHybrid.Add), nameof(DotnetHybrid.Add) + "Package._Source")] = "The NuGet package source to use during the restore.",
            [(typeof(DotnetHybrid.Add), nameof(DotnetHybrid.Add) + "Package._NoRestore")] = "Add the reference without performing restore preview and compatibility check.",
            [(typeof(DotnetHybrid.Add), nameof(DotnetHybrid.Add) + "Package.Interactive")] = addFrameworkHelp,
            [(typeof(DotnetHybrid.Add), nameof(DotnetHybrid.Add) + "Package.PackageDirectory")] = "The directory to restore packages to.",
            [(typeof(DotnetHybrid.Add), nameof(DotnetHybrid.Add) + "Reference.ProjectPath")] = "The paths to the projects to add as references.",
            [(typeof(DotnetHybrid.Add), nameof(DotnetHybrid.Add) + "Reference.Framework")] = addFrameworkHelp,
            [(typeof(DotnetHybrid.Remove), nameof(Remove.Package) + "PackageNameArg")] = "The package reference to remove.",
            [(typeof(DotnetHybrid.Remove), nameof(Remove.Reference) + "ProjectPathArg")] = "The paths to the referenced projects to remove.",
            [(typeof(DotnetHybrid.Remove), nameof(Remove.Reference) + "Framework")] = removeFrameworkHelp,
        };

        public string Description<TSource>(TSource source)
        {
            var help = "";
            if (!(source is Type resultType && commandHelp.TryGetValue(resultType, out help)))
            {
                if (source is MethodInfo methodInfo)
                {
                    symbolHelp.TryGetValue((methodInfo.DeclaringType, methodInfo.Name), out help);
                }
            }
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
