using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Text;

namespace ApiSamples
{
    public class Fixer
    {
//Fix
//  --volume  Volume to fix
//  --fix|f   Actually fix
//  --log|l   Log actions";

        public void Parse()
        {
            var result = GetBuilder().Parse("Fix --volume");
        }

        private Parser GetBuilder()
        {
            var builder = new CommandLineBuilder();
            builder.AddCommand("Fix", "Fix something about the disk",
                           cmd => cmd.AddOption(new[] { "--volume", "Volume to fix" }));

            return builder.Build();
        }
    }
}
