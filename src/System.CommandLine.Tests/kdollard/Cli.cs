using System;
using System.Collections.Generic;
using System.Text;

namespace System.CommandLine.Tests.kdollard
{
    public class Cli
    {
        public Project Project {get; }
        public Tool Tool {get;}
}

    public class Tool
    {
    }

    public class Project
    {
        // Logic for using nested classes instead of strings is to provide
        // more information on intent. But this may not be the way because
        // Add/Remove would autocomplete in fundamentally different ways. 
        public string ProjectPath { get; }
        public void Add(Reference reference) { }
        public void Add(Package package) { }

        public void Remove(Reference reference) { }
        public void Remove(Package package) { }

        public class Reference
        {
            public string ProjectFile { get; }
        }

        public class Package
        {
            // Three approaches to prepping for autoComplete:
            // - Class that allows evaluation on type. Common ones like PackageName would be provided
            // - Comments and notes in comments/Ref to ensure typing
            // - Attributes
            public PackageName PackageName { get; }

            /// <summary>
            ///  Version for the package to be added.
            /// </summary>
            /// <abbreviation>v</abbreviation>
            public string Version { get; }

               /// <summary>
               /// Adds reference only when targeting a specific framework.
               /// </summary>
               /// <abbreviation>f</abbreviation>
               /// <automcomplete>Something with Ref</automcomplete>
               public string Framework { get; }

[Option( "Adds reference without performing restore preview and compatibility check.", "n")]
            public bool NoRestore { get; }

            [Autocomplete(typeof(SourceAutoComplete)]
            [Option("Specifies NuGet package sources to use during the restore.", "s")]
            public string Source { get; }
  --package-directory<PACKAGE_DIRECTORY> Restores the packages to the specified directory.

        }
        public class PackageName
        {
        }
    }

    internal class OptionAttribute : Attribute
    {
    }

    internal class AutocompleteAttribute : Attribute
    {
    }

    internal class SourceAutoComplete
    {
    }
}
