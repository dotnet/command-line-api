using System;
using System.Collections.Generic;
using System.Text;

namespace System.CommandLine.Tests.kdollard
{
    // Have leaf commands inherit from the trunk commands, allowing us differentiate at design time and conflate
    // at runtime.

    // 

    abstract class Command
    {
        Command Parent;

        //Command ChildCalledCommand;

        void Main()
        {
            Parse
            Invoke
        }

        public abstract void Invocation();
    }

    //class ParseResult
    //{
    //    Command Command;
    //}
    abstract class DotNetCommand : Command
    { 
       
    }

    [Help]
    class FileCommand : DotNetCommand
    {
        public FileCommand(string fileName, IEnumerable<string> otherNames)
        {
            FileName = fileName;
            OtherNames = otherNames;
        }

        // For arguments, always null, and no abbrev
        // Argument names get expanded to 'PROJECT_NAME', option names to 'projectName'
        [Argument(Help)]
        string ProjectName { get; }


        // Decide whether to use three attributes or a single attribute
        [Option(abbrev)]
        [help]
        [nullAllowed]
        string FileName { get; }

        [Option(abbrev, help, emptyAllowed)]
        IEnumerable<string> OtherNames {  get; }


        public void Invoke()
        {
            if (IO.File.Exists(FileName))
            {
                DoTheThing();
            }
        }
    }
    class Meeting
    {

    }
}
