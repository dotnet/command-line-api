using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace System.CommandLine.JackFruit
{
    public static class Runner
    {
        public static async Task< int> RunAsync<TCli, THelper>(string[] args)
        {
            var builder = BuilderTools.Create<TCli, THelper>()
                          .AddStandardDirectives()
                          .UseExceptionHandler();
            // Create approach to add extra stuff
            Parser parser = builder.Build();
            return await parser.InvokeAsync(args);

        }

  
    }
}
