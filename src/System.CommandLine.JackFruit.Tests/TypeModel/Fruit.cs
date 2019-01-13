using System.Threading.Tasks;

namespace System.CommandLine.JackFruit.Tests.MethodModel
{
    public class FruitType
    {
        public static string Captured;

        public string Melon { get; set; }
        public bool   Berry { get; set; }
        public int    Mango { get; set; }
        public string Banana { get; set; }

        public Task<int> InvokeAsync()
        {
            Captured =
                $@"Melon = {Melon}
Berry = {Berry}
Mango = {Mango}
Banana = {Banana}";
            return Task.FromResult(43);
        }
    }
}
