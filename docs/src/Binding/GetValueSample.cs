// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using static Binding.Formatter;

namespace Binding
{
    internal static class GetValueSample
    {
        public static int GetValueFromOptionArgument()
        {
            #region GetValueFromOptionArgument

            var option = new Option<int>("--an-int");
            ParseResult parseResult = option.Parse("--an-int 123");
            int value = parseResult.ValueForOption(option);
            Console.WriteLine(Format(value));

            #endregion

            return 0;
        }
    }
}
