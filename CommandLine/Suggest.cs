using System.Collections.Generic;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public delegate IEnumerable<string> Suggest(ParseResult parseResult);
}