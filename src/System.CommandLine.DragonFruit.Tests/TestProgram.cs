// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;

namespace System.CommandLine.DragonFruit.Tests
{
    public class TestProgram
    {
        public static readonly MethodInfo TestMainMethodInfo = typeof(TestProgram).GetMethod(nameof(TestMain));

        public static readonly MethodInfo TestMainMethodInfoWithDefault = typeof(TestProgram).GetMethod(nameof(TestMainWithDefault));

        /// <summary>
        /// Help for the test program
        /// </summary>
        /// <param name="name">Specifies the name option</param>
        /// <param name="console"></param>
        /// <param name="args">These are arguments</param>
        public void TestMain(string name, IConsole console, string[] args = null)
        {
            console.Out.Write(name);
            if (args != null && args.Length > 0)
            {
                console.Out.Write($"args: { string.Join(",", args) }");
            }
        }

        public void TestMainWithDefault(string name = "Bruce", IConsole console = null)
        {
            console?.Out.Write(name);
        }
    }
}
