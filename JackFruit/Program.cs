using System.IO;
using System.CommandLine.JackFruit;

namespace JackFruit
{
    class Program
    {
        static async void Main(string[] args)
        {
            // Approach #1
            await Runner.RunAsync<DotJackFruit, DotJackFruitHelpTextProvider>(args);

            // Approach #2
            DotJackFruit dot = ResultTools.GetResult<DotJackFruit, DotJackFruitHelpTextProvider>(args);
            switch (dot)
            {
                case ToolInstall toolInstall :
                    ToolActions.Install(toolInstall.Global,toolInstall.ToolPath, toolInstall.Version,
                        toolInstall.ConfigFile, toolInstall.AddSource, toolInstall.Framework, toolInstall.Verbosity );
                    break;
            }

        }
    }
}
