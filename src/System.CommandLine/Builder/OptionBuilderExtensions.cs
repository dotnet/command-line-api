using System;
using System.Collections.Generic;
using System.Text;

namespace System.CommandLine.Builder
{
    public static class OptionBuilderExtensions
    {
        public static Option WithHelp(
            this Option option,
            string name = null,
            string description = null,
            bool isHidden = HelpDetail.DefaultIsHidden)
        {
            option.Help = new HelpDetail(name, description, isHidden);

            return option;
        }
    }
}
