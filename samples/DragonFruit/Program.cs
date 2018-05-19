using System;

namespace DragonFruitSample
{
    class Program
    {
        /// <summary>
        /// Hello
        /// </summary>
        /// <param name="flavor"></param>
        /// <param name="count"></param>
        static void Main(
            string flavor,
            int count = 1)
        {
            Console.WriteLine($"Creating {count} banana {(count == 1 ? "smoothie" : "smoothies")} with {flavor}");
        }
    }
}
