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
        public void TestMain(string name)
        {
            Captured = name;
        }

        public void TestMainWithDefault(string name = "Bruce")
        {
            Captured = name;
        }

        public string Captured { get; private set; }
    }
}
