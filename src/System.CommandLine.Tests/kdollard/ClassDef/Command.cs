using System;
using System.Collections.Generic;
using System.Text;

namespace System.CommandLine.Tests.kdollard.ClassDef
{
    public abstract class Command
    {
        public abstract void Invoke();
        public bool IsDefault {get; protected set;} = False;
    }
}
