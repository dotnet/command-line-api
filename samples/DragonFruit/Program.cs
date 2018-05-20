using System;

namespace DragonFruit
{
    class Program
    {
        /// <summary>
        /// Hello
        /// </summary>
        /// <param name="flavor"></param>
        /// <param name="count"></param>
        static void Main(
            bool verbose,
            string flavor = "chocolate",
            int count = 1)
        {
            if (verbose)
            {
                Console.WriteLine("Running in verbose mode");
            }
            Console.WriteLine($"Creating {count} banana {(count == 1 ? "smoothie" : "smoothies")} with {flavor}");
        }
    }
}
