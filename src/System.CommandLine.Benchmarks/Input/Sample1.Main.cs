// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/*
 * NOTE: The build action for this file is 'None'!
 */

using System;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace RenderingPlayground
{
    class Program
    {
        /// <summary>
        /// Demonstrates various rendering capabilities.
        /// </summary>
        /// <param name="invocationContext"></param>
        /// <param name="sample">&lt;colors|dir&gt; Renders a specified sample</param>
        /// <param name="height">The height of the rendering area</param>
        /// <param name="width">The width of the rendering area</param>
        /// <param name="top">The top position of the render area</param>
        /// <param name="left">The left position of the render area</param>
        /// <param name="text">The text to render</param>
        /// <param name="overwrite">Overwrite the specified region. (If not, scroll.)</param>
        public static void Main(
#pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)
            ParseResult parseResult,
            SampleName sample = SampleName.Dir,
            int? height = null,
            int? width = null,
            int top = 0,
            int left = 0,
            string text = null,
            bool overwrite = true)
#pragma warning restore CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)
        {
 
        }
    }

    internal enum SampleName
    {
        Colors,
        Dir,
        Moby,
        Processes,
        TableView,
        Clock,
        GridLayout,
    }
}
