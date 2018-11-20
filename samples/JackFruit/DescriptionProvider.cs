using System;
using System.Collections.Generic;
using System.CommandLine.JackFruit;

namespace JackFruit
{
    internal class DescriptionProvider : IDescriptionProvider<Type>
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
            [(typeof(ToolInstall), "PackageId")] = "The NuGet Package Id of the tool to install.",
            [(typeof(ToolInstall), "Global")] = "Install a tool for use on the command line.",
            [(typeof(ToolInstall), "ToolPath")] = "The directory where the tool will be installed.The directory will be created if it does not exist.",
            [(typeof(ToolInstall), "Version")] = "The version of the tool package to install.",
            [(typeof(ToolInstall), "ConfigFile")] = configFileHelp,
            [(typeof(ToolInstall), "AddPackageSource")] = "Add an additional NuGet package source to use during installation.",
            [(typeof(ToolInstall), "Framework")] = "The target framework to install the tool for.",
            [(typeof(ToolInstall), "Verbosity")] = verbosityHelp,
            [(typeof(ToolUpdate), "PackageId")] = "The NuGet Package Id of the tool to install.",
            [(typeof(ToolUpdate), "Global")] = "Update the tool in the current user's tools directory.",
            [(typeof(ToolUpdate), "ToolPath")] = "The directory containing the tool to update.",
            [(typeof(ToolUpdate), "ConfigFile")] = configFileHelp,
            [(typeof(ToolUpdate), "AddPackageSource")] = "Add an additional NuGet package source to use during update.",
            [(typeof(ToolUpdate), "Framework")] = "The target framework to update the tool for.",
            [(typeof(ToolUpdate), "Verbosity")] = verbosityHelp,
            [(typeof(ToolUninstall), "PackageId")] = "The NuGet Package Id of the tool to install.",
            [(typeof(ToolUninstall), "Global")] = "Uninstall the tool from the current user's tools directory.",
            [(typeof(ToolUninstall), "ToolPath")] = "The directory containing the tool to uninstall.",
            [(typeof(ToolList), "Global")] = "List tools in the current user's tools directory.",
            [(typeof(ToolList), "ToolPath")] = "The directory containing the tools to list.",
            [(typeof(Add), "ProjectFile")] = projectFileHelp,
            [(typeof(Remove), "ProjectFile")] = projectFileHelp,
            [(typeof(List), "ProjectFile")] = projectFileHelp,
            [(typeof(Add.Package), "PackageName")] = "The package reference to add.",
            [(typeof(Add.Package), "Framework")] = addFrameworkHelp,
            [(typeof(Add.Package), "Source")] = "The NuGet package source to use during the restore.",
            [(typeof(Add.Package), "NoRestore")] = "Add the reference without performing restore preview and compatibility check.",
            [(typeof(Add.Package), "Interactive")] = addFrameworkHelp,
            [(typeof(Add.Package), "PackageDirectory")] = "The directory to restore packages to.",
            [(typeof(Add.Reference), "ProjectPath")] = "The paths to the projects to add as references.",
            [(typeof(Add.Reference), "Framework")] = addFrameworkHelp,
            [(typeof(Remove.Package), "PackageName")] = "The package reference to remove.",
            [(typeof(Remove.Reference), "ProjectPath")] = "The paths to the referenced projects to remove.",
            [(typeof(Remove.Reference), "Framework")] = removeFrameworkHelp,
        };

        public string Description(Type resultType)
        {
            if (commandHelp.TryGetValue(resultType, out string help))
            {
                return help;
            }
            return "";
        }

        public string Description(Type resultType, string name)
        {
            if (symbolHelp.TryGetValue((resultType, name), out string help))
            {
                return help;
            }
            return "";
        }
    }
}
