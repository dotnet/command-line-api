// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace DragonFruit
{
    class Program
    {
        /// <summary>
        /// DragonFruit simple example program
        /// </summary>
        /// <param name="verbose">Show verbose output</param>
        /// <param name="flavor">Which flavor to use</param>
        /// <param name="count">How many smoothies?</param>
        static int Main(
            bool verbose,
            string flavor = "chocolate",
            int count = 1)
        {
            if (verbose)
            {
                Console.WriteLine("Running in verbose mode");
            }
            Console.WriteLine($"Creating {count} banana {(count == 1 ? "smoothie" : "smoothies")} with {flavor}");
            return 0;
        }
    }
}
