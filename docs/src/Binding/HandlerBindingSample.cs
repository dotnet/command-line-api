// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.IO;
using System.Threading.Tasks;
using static Binding.Formatter;

namespace Binding
{
    internal static class HandlerBindingSample
    {
        internal static async Task<int> MultipleArgs()
        {
            #region MultipleArgs

            var command = new RootCommand
            {
                new Option<string>("--a-string"),
                new Option<int>("--an-int")
            };

            command.Handler = CommandHandler.Create(
                (string aString, int anInt) =>
                {
                    Console.WriteLine($"{aString}");
                    Console.WriteLine($"{anInt}");
                });

            await command.InvokeAsync("--an-int 123 --a-string \"Hello world!\" ");

            #endregion

            return 0;
        }

        internal static async Task<int> Bool()
        {
            #region Bool

            var command = new RootCommand
            {
                new Option<bool>("--a-bool")
            };

            command.Handler = CommandHandler.Create(
                (bool aBool) => Console.WriteLine(aBool));

            await command.InvokeAsync("");
            await command.InvokeAsync("--a-bool");
            await command.InvokeAsync("--a-bool false");
            await command.InvokeAsync("--a-bool true");

            #endregion

            return 0;
        }

        internal static async Task<int> DependencyInjection()
        {
            #region DependencyInjection

            var command = new RootCommand
            {
                new Option<string>("--a-string"),
                new Option<int>("--an-int"),
                new Option<System.IO.FileAttributes>("--an-enum"),
            };

            command.Handler = CommandHandler.Create(
                (ParseResult parseResult, IConsole console) =>
                {
                    console.Out.WriteLine($"{parseResult}");
                });

            await command.InvokeAsync("--an-int 123 --a-string \"Hello world!\" --an-enum compressed");
            
            #endregion

            return 0;
        }

        internal static async Task<int> Enum()
        {
            #region Enum

            var command = new RootCommand
            {
                new Option<System.IO.FileAccess>("--an-enum")
            };

            command.Handler = CommandHandler.Create(
                (FileAccess anEnum) => Console.WriteLine(anEnum));

            await command.InvokeAsync("--an-enum Read");
            await command.InvokeAsync("--an-enum READ");

            #endregion

            return 0;
        }

        internal static async Task<int> Enumerables()
        {
            #region Enumerables

            var command = new RootCommand
            {
                new Option<string[]>("--items")
            };

            command.Handler = CommandHandler.Create(
                (IEnumerable<string> items) => 
                {
                    Console.WriteLine(items.GetType());

                    foreach (var item in items) 
                    {
                        Console.WriteLine(item);
                    }
                });

            await command.InvokeAsync("--items one two three");

            #endregion

            return 0;
        }

        internal static async Task<int> FileSystemTypes()
        {
            #region FileSystemTypes

            var command = new RootCommand
            {
                new Option<FileInfo>("-f").ExistingOnly()
            };

            command.Handler = CommandHandler.Create(
                (FileSystemInfo f) =>
                {
                    Console.WriteLine($"{f.GetType()}: {f}");
                });

            await command.InvokeAsync("-f /path/to/something");

            #endregion

            return 0;
        }

        #region ComplexTypes
        
        public static async Task<int> ComplexTypes()
        {
            var command = new Command("the-command")
            {
                new Option<int>("--an-int"),
                new Option<string>("--a-string") 
            };

            command.Handler = CommandHandler.Create(
                (ComplexType complexType) =>
                {
                    Console.WriteLine(Format(complexType));
                });
            
            await command.InvokeAsync("--an-int 123 --a-string 456");

            return 0;
        }

        public class ComplexType
        {
            // public ComplexType(int anInt, string aString)
            // {
            //     AnInt = anInt;
            //     AString = aString;
            // }
            public int AnInt { get; set; }
            public string AString { get; set; }
        }

        #endregion
    }
}
