using System;
using System.Collections.Generic;
using System.CommandLine.JackFruit;
using System.Reflection;

namespace System.CommandLine.JackFruit.Tests
{
    internal class DescriptionFinder : IDescriptionFinder
    {
        private Dictionary<Type, string> commandHelp = new Dictionary<Type, string>()
        {
            [typeof(Tool)] = "Install or manage tools that extend the .NET experience.",
            [typeof(ToolInstall)] = "Install a tool for use on the command line.",
            [typeof(ToolUninstall)] = "Uninstall a tool from the current development environment.",
            [typeof(ToolUpdate)] = "Update a tool to the latest stable version.",
            [typeof(ToolList)] = "List tools installed in the current development environment. ",
            [typeof(Add)] = "Add a package or reference to a .NET project.",
            [typeof(Remove)] = "Remove a package or reference from a .NET project.",
            [typeof(List)] = "List project references of a .NET project.",
            [typeof(Add.Package)] = "Add a NuGet package reference to the project.",
            [typeof(Add.Reference)] = "Add a project-to-project reference to the project.",
            [typeof(Remove.Package)] = "Remove a NuGet package reference from the project.",
            [typeof(Remove.Reference)] = "Remove a project-to-project reference from the project.",
            [typeof(List.Package)] = "List all NuGet packages for the project.",
            [typeof(List.Reference)] = "List all project-to-project references of the project.",
        };

        private const string verbosityHelp = "Set the MSBuild verbosity level.Allowed values are q[uiet], m[inimal], n[ormal], d[etailed], and diag[nostic]";
        private const string configFileHelp = "The NuGet configuration file to use.";
        private const string projectFileHelp = "The project file to operate on. If a file is not specified, the command will search the current directory for one.";
        private const string addFrameworkHelp = "Add the reference only when targeting a specific framework.";
        private const string removeFrameworkHelp = "Remove the reference only when targeting a specific framework.";
        private Dictionary<(Type, string), string> symbolHelp = new Dictionary<(Type, string), string>()
        {
            [(typeof(ToolInstall), nameof(ToolInstall.PackageId))] = "The NuGet Package Id of the tool to install.",
            [(typeof(ToolInstall), nameof(ToolInstall.Global))] = "Install a tool for use on the command line.",
            [(typeof(ToolInstall), nameof(ToolInstall.ToolPath))] = "The directory where the tool will be installed.The directory will be created if it does not exist.",
            [(typeof(ToolInstall), nameof(ToolInstall.Version))] = "The version of the tool package to install.",
            [(typeof(ToolInstall), nameof(ToolInstall.ConfigFile))] = configFileHelp,
            [(typeof(ToolInstall), nameof(ToolInstall.AddSource))] = "Add an additional NuGet package source to use during installation.",
            [(typeof(ToolInstall), nameof(ToolInstall.Framework))] = "The target framework to install the tool for.",
            [(typeof(ToolInstall), nameof(ToolInstall.Verbosity))] = verbosityHelp,
            [(typeof(ToolUpdate), nameof(ToolUpdate.PackageId))] = "The NuGet Package Id of the tool to install.",
            [(typeof(ToolUpdate), nameof(ToolUpdate.Global))] = "Update the tool in the current user's tools directory.",
            [(typeof(ToolUpdate), nameof(ToolUpdate.ToolPath))] = "The directory containing the tool to update.",
            [(typeof(ToolUpdate), nameof(ToolUpdate.ConfigFile))] = configFileHelp,
            [(typeof(ToolUpdate), nameof(ToolUpdate.AddSource))] = "Add an additional NuGet package source to use during update.",
            [(typeof(ToolUpdate), nameof(ToolUpdate.Framework))] = "The target framework to update the tool for.",
            [(typeof(ToolUpdate), nameof(ToolUpdate.Verbosity))] = verbosityHelp,
            [(typeof(ToolUninstall), nameof(ToolUninstall.PackageId))] = "The NuGet Package Id of the tool to install.",
            [(typeof(ToolUninstall), nameof(ToolUninstall.Global))] = "Uninstall the tool from the current user's tools directory.",
            [(typeof(ToolUninstall), nameof(ToolUninstall.ToolPath))] = "The directory containing the tool to uninstall.",
            [(typeof(ToolList), nameof(ToolList.Global))] = "List tools in the current user's tools directory.",
            [(typeof(ToolList), nameof(ToolList.ToolPath))] = "The directory containing the tools to list.",
            [(typeof(Add), nameof(Add.ProjectFile))] = projectFileHelp,
            [(typeof(Remove), nameof(Remove.ProjectFileArg))] = projectFileHelp,
            [(typeof(List), nameof(List.ProjectFile))] = projectFileHelp,
            [(typeof(Add.Package), nameof(Add.Package.PackageName))] = "The package reference to add.",
            [(typeof(Add.Package), nameof(Add.Package._Framework))] = addFrameworkHelp,
            [(typeof(Add.Package), nameof(Add.Package._Source))] = "The NuGet package source to use during the restore.",
            [(typeof(Add.Package), nameof(Add.Package._NoRestore))] = "Add the reference without performing restore preview and compatibility check.",
            [(typeof(Add.Package), nameof(Add.Package.Interactive))] = addFrameworkHelp,
            [(typeof(Add.Package), nameof(Add.Package.PackageDirectory))] = "The directory to restore packages to.",
            [(typeof(Add.Reference), nameof(Add.Reference.ProjectPath))] = "The paths to the projects to add as references.",
            [(typeof(Add.Reference), nameof(Add.Reference.Framework))] = addFrameworkHelp,
            [(typeof(Remove.Package), nameof(Remove.Package.PackageNameArg))] = "The package reference to remove.",
            [(typeof(Remove.Reference), nameof(Remove.Reference.ProjectPathArg))] = "The paths to the referenced projects to remove.",
            [(typeof(Remove.Reference), nameof(Remove.Reference.Framework))] = removeFrameworkHelp,
        };

        public string Description<TSource>(TSource source)
            => source is Type resultType
                && commandHelp.TryGetValue(resultType, out string help)
                    ? help
                    : "";

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
