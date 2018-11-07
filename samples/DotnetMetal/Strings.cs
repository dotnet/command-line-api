using System;
using System.Collections.Generic;
using System.Text;

namespace DotMetal
{
    class Strings
    {
        public static readonly string sdkDescription = "Sdk description";
        public static readonly string toolDescription = "Tool description";
        public static readonly string sdkInstallDescription = "Sdk install description";
        public static readonly string sdkListDescription = "Sdk list description";
        public static readonly string sdkUpdateDescription = "Sdk update description";
        public static readonly string sdkUninstallDescription = "Sdk unintsall description";
        public static readonly string toolInstallDescription = "Tool install description";
        public static readonly string toolListDescription = "Tool list description";
        public static readonly string toolUpdateDescription = "Tool update description";
        public static readonly string toolUninstallDescription = "Tool unintsall description";
        public static readonly string toolGlobalOptionDescription = "Install the tool to the current user's tools directory.";
        public static readonly string toolPathOptionDescription = "The directory where the tool will be installed. The directory will be created if it does not exist.";
        public static readonly string versionOptionDescription = "The version of the tool package to install.";
        public static readonly string configFileOptionDescription = "The NuGet configuration file to use.";
        public static readonly string addSourceOptionDescription = "Add an additional NuGet package source to use during installation.";
        public static readonly string frameworkOptionDescription = "The target framework to install the tool for.";
        public static readonly string verbosityOptionDescription = "Set the MSBuild verbosity level. Allowed values are q[uiet], m[inimal], n[ormal], d[etailed], and diag[nostic].";

        public static readonly string rootVersionOptionDescription = "Display .NET Core SDK version in use.";
        public static readonly string rootInfoOptionDescription = "Display .NET Core information.";
        public static readonly string rootListSdksOptionDescription = "Display the installed SDKs.";
        public static readonly string rootListRuntimesOptionDescription = "Display the installed runtimes.";
    }
}
