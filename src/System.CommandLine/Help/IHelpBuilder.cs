// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;

namespace System.CommandLine.Help
{
    public interface IHelpBuilder
    {
        void Write(ICommand command, TextWriter output);
    }

    //public static class HelpBuilderExtensions
    //{
    //    public static void Write(this IHelpBuilder builder, ICommand command)
    //    {
    //        builder.Write(command, Console.Out);
    //    }
    //}
}
