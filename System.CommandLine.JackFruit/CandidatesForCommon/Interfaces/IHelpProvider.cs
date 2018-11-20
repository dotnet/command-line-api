using System;
using System.Collections.Generic;
using System.Text;

namespace System.CommandLine.JackFruit
{
    public interface IHelpProvider<TSource>
    {
        string GetHelp(TSource source);
        string GetHelp(TSource source, string name);
    }

    public interface IHelpProvider<TSource, TItem >: IHelpProvider<TSource>
    {
        string GetHelp(TSource source, TItem item);
    }
}
