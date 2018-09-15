using System;
using System.Collections.Generic;
using System.Text;

namespace System.CommandLine.Builder
{
    public static class OptionBuilderExtensions
    {
        public static OptionBuilder WithHelp(this OptionBuilder builder, HelpDetail help)
        {
            builder.Help = help;

            return builder;
        }
    }
}
