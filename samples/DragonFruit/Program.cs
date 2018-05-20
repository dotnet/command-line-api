﻿using System;

namespace DragonFruit
{
    class Program
    {
        /// <summary>
        /// Hello
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
            return 1;
        }
    }
}
