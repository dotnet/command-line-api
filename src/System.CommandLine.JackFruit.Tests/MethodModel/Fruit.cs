using System;
using System.Collections.Generic;
using System.Text;

namespace System.CommandLine.JackFruit.Tests.MethodModel
{
    public class Fruit
    {
        public static string Captured;

        public int Bowl(
            string melon,
            bool berry,
            int mango,
            string banana)
        {
            Captured =
                $@"Melon: {melon}
Berry = {berry}
Mango = {mango}
Banana = {banana}";
            return 43;
        }
    }
}
