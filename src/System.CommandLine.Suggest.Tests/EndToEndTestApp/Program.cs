using System.CommandLine;
/*
using System.CommandLine.Help;
*/
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using System.Threading;

namespace EndToEndTestApp
{
    public class Program
    {
        static void Main(string[] args)
        {
            CliOption<string> appleOption = new ("--apple" );
            CliOption<string> bananaOption = new ("--banana");
            CliOption<string> cherryOption = new ("--cherry");
            CliOption<string> durianOption = new ("--durian");

            CliRootCommand rootCommand = new ()
            {
                appleOption,          
                bananaOption,          
                cherryOption,          
                durianOption,
            };

            CliConfiguration commandLine = new (rootCommand);

            var result = CliParser.Parse(commandLine.RootCommand, args, commandLine);

        }
    }
}
