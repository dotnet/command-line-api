using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Linq;

namespace ObjectBinder
{
    public interface IObjectBindingModel
    {
        ParseResult ParseResult { get; }
        bool HelpRequested { get; }
    }
}