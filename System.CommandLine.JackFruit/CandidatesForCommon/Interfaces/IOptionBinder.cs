using System;
using System.Collections.Generic;
using System.Text;

namespace System.CommandLine.JackFruit
{
    public interface IOptionBinder<TParent, TSource>
    {
        Option GetOption(TParent parent, TSource source);

        // See note in ICommandProvider on avoiding type explosion
        IHelpProvider<TParent, TSource> HelpProvider { get; set; }
        string GetName(TParent parent, TSource source);
        string GetHelp(TParent parent, TSource source);
    }
}
