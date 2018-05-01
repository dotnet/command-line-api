// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public class ArgumentRuleBuilder<T> : ArgumentRuleBuilder
    {
        public ArgumentRuleBuilder()
            : this(FigureMeOut())
        {
        }

        private static ConvertArgument FigureMeOut()
        {
            //TODO: Jump table
            if (typeof(T) == typeof(string))
            {
                return symbol => ArgumentParseResult.Success(symbol.Token);
            }

            throw new NotImplementedException();
        }

        public ArgumentRuleBuilder(ConvertArgument convertArgument)
        {
            ArgumentParser = new ArgumentParser<T>(convertArgument);
        }

        protected override ArgumentParser BuildArgumentParser()
            => ArgumentParser;

        public ArgumentParser<T> ArgumentParser { get; set; }
    }
}