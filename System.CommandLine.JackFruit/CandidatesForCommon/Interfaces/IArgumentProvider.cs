using System;
using System.Collections.Generic;
using System.Text;

namespace System.CommandLine.JackFruit
{
    public interface IArgumentProvider<TSource,TItem>
    {
        Argument GetArgument(TSource source);

        // See note in ICommandProvider on avoiding type explosion
        IHelpProvider<TSource> HelpProvider { get; set; }
        string GetName(TSource source);
        string GetHelp(TSource source);
        bool IsArgument(TSource source, TItem item);
    }
}
