using System.IO;
using System.CommandLine.JackFruit;

namespace JackFruit
{
    class Program
    {
        static async void Main(string[] args)
        {
            // Approach #1
            await Runner.RunAsync<DotnetJackFruit, DotnetJackFruitHelpTextProvider>(args);

            // Approach #2
            DotnetJackFruit dot = ResultTools.GetResult<DotnetJackFruit, DotnetJackFruitHelpTextProvider>(args);
            switch (dot)
            {
                case ToolInstall toolInstall :
                    ToolActions.InstallAsync(toolInstall.Global,toolInstall.ToolPath, toolInstall.Version,
                        toolInstall.ConfigFile, toolInstall.AddSource, toolInstall.Framework, toolInstall.Verbosity );
                    break;
            }

        }
    }
}
