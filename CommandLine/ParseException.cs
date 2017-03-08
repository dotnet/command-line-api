using System;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public class ParseException : Exception
    {
        public ParseException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}