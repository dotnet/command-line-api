using System;
using System.Collections.Generic;
using System.Text;

namespace System.CommandLine
{
    /// <summary>
    /// Exception to capture issues related to not being able to find an <see cref="IOption"/>
    /// or <see cref="IArgument"/> with a desired alias.
    /// </summary>
    public class UnknownAliasException : Exception
    {
        /// <summary>
        /// Creates the instance.
        /// </summary>
        /// <param name="alias">the alias which could not be found</param>
        /// <param name="forOption">true to indicate the problem involved IOptions, false to indicate IArguments</param>
        public UnknownAliasException( string alias, bool forOption )
        {
            Alias = alias;
            ForOption = forOption;
        }

        public string Alias { get; }
        public bool ForOption { get; }
    }
}
