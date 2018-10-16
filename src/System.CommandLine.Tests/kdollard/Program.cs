using System;
using System.Collections.Generic;
using System.Text;

namespace System.CommandLine.Tests.kdollard
{
    class Program
    {
        // SuperDragonFruit needs a new name
        public int Main ()
        {
            var result = Command.Parse<DotNetCommand>();
            return result.Command.Invocation();
            
        }



        // DragonFruit
        public Main (int FirstThing)
        { }


        //public Main(FileCommand fileCommand)
        //{

        //}

        //public Main(NotFileCommand notFileCommand)
        //{

        //}
    }
}
