using System.Collections.Generic;

namespace System.CommandLine
{
    public class LexResult
    {
        public IEnumerable<Token> Tokens { get; set; }
        public IEnumerable<ParseError> Errors { get; set; }
    }
}
