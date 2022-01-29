// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;

namespace System.CommandLine.DragonFruit.Tests
{
    public class TestProgram
    {
        public static readonly MethodInfo TestMainMethodInfoWithoutPara = typeof(TestProgram).GetMethod(nameof(TestMainWithoutPara));
        
        public static readonly MethodInfo TestMainMethodInfoWithPara = typeof(TestProgram).GetMethod(nameof(TestMainWithPara));

        public static readonly MethodInfo TestMainMethodInfoWithDefault = typeof(TestProgram).GetMethod(nameof(TestMainWithDefault));

        /// <summary>
        /// <para>Help for the test program</para>
        /// <para>More help for the test program</para>
        /// </summary>
        /// <param name="name">Specifies the name option</param>
        /// <param name="console"></param>
        /// <param name="args">These are arguments</param>
        public void TestMainWithPara(string name, IConsole console, string[] args = null)
        {
            console.Out.Write(name);
            if (args != null && args.Length > 0)
            {
                console.Out.Write($"args: { string.Join(",", args) }");
            }
        }

        /// <summary>
        /// Normal summary
        /// </summary>
        /// <param name="name">Specifies the name option</param>
        /// <param name="console"></param>
        /// <param name="args">These are arguments</param>
        public void TestMainWithoutPara(string name, IConsole console, string[] args = null)
        {
            console.Out.Write(name);
        }

        public void TestMainWithDefault(string name = "Bruce", IConsole console = null)
        {
            console?.Out.Write(name);
        }
    }
}
