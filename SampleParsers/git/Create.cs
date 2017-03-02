using System;
using System.Linq;
using static Microsoft.DotNet.Cli.CommandLine.Create;

namespace Microsoft.DotNet.Cli.CommandLine.SampleParsers.git
{
    public class Create
    {
        public static Parser Parser()
        {
            return new Parser(
                Command("git", "",
                        Command("status", "",
                                Option("-v|--verbose",
                                       "be verbose"),
                                Option("-s|--short",
                                       help: "show status concisely"),
                                Option("-b|--branch",
                                       help: "show branch information"),
                                Option("--porcelain[=<version>]",
                                       help: "machine-readable output"),
                                Option("--long",
                                       help: "show status in long format (default)"),
                                Option("-z|--null",
                                       help: "terminate entries with NUL"),
                                Option("-u|--untracked-files[=<mode>]",
                                       help: "show untracked files, optional modes: all, normal, no. (Default: all)"),
                                Option("--ignored",
                                       help: "show ignored files"),
                                Option("--ignore-submodules[=<when>]",
                                       help: "ignore changes to submodules, optional when: all, dirty, untracked. (Default: all)"),
                                Option("--column[=<style>]",
                                       help: "list untracked files in columns"),
                                Option("--no-lock-index",
                                       help: "do not lock the index"))));
        }
    }
}