using System.Collections.Generic;

namespace CommandLine
{
    public delegate IEnumerable<string> Suggest(ParseResult parseResult);
}