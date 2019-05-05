// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;

namespace System.CommandLine.Tests
{
    public partial class ParserTests
    {
        public class MultipleArguments
        {
            [Fact(Skip = "#310")]
            public void Mulitple_positional_arguments_can_differ_by_arity()
            {
                // FIX: (Mulitple_positional_arguments_can_differ_by_arity) 
                var command = new Command("the-command")
                {
                    // new Argument<string>()
                };
                
                // TODO-JOSEQU (Mulitple_positional_arguments_can_differ_by_arity) write test
                Assert.True(false, "Test Mulitple_positional_arguments_can_differ_by_arity is not written yet.");
            }
        }
    }
}
