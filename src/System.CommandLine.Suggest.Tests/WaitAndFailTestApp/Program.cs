using System;
using System.Threading.Tasks;

namespace WaitAndFailTestApp;

public class Program
{
    private static TimeSpan defaultWait = TimeSpan.FromMilliseconds(3000);
    
    //we should not be able to receive any suggestion from this test app,
    //so we are not constructing it using CliConfiguration
    
    static async Task Main(string[] args)
    {
        var waitPeriod = args.Length > 0 && int.TryParse(args[0], out var millisecondsToWaitParsed)
            ? TimeSpan.FromMilliseconds(millisecondsToWaitParsed)
            : defaultWait; 
        
        await Task.Delay(waitPeriod);
        Environment.ExitCode = 1;
        
        Console.WriteLine("this 'suggestion' is provided too late and/or with invalid app exit code");
    }
}

