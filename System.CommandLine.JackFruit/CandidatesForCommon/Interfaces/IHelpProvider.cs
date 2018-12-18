using System;
using System.Collections.Generic;
using System.Text;

namespace System.CommandLine.JackFruit
{
    public interface IHelpProvider<TSource>
    {
        string GetHelp(TSource source);
        string GetHelp<TItem>(TSource source, TItem item);
    }
}
