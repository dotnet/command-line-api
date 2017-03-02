// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Text;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public class CommandExecutionResult
    {
        private readonly ParseResult parseResult;

        public CommandExecutionResult(ParseResult parseResult, object value = null)
        {
            if (parseResult == null)
            {
                throw new ArgumentNullException(nameof(parseResult));
            }
            this.parseResult = parseResult;
        }

        public int Code => parseResult.Errors.Any()
                               ? 1
                               : 0;

        public override string ToString()
        {
            if (parseResult.Errors.Any())
            {
                var builder = new StringBuilder();

                foreach (var error in parseResult.Errors)
                {
                    builder.AppendLine(error.ToString());
                }

                builder.Append(parseResult.Command().HelpView());

                return builder.ToString();
            }

            return "";
        }
    }
}