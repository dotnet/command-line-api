using System.CommandLine;
using System.CommandLine.Hosting;
using System.CommandLine.NamingConventionBinder;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using HostingPlayground;

CliOption<string> cliNameOption = new("--name", "-n")
{
    Required = true,
};
CliRootCommand cliRootCommand = new(@"$ dotnet run --name 'Joe'")
{
    Action = HostedServiceAction.Create<Greeter>(host =>
    {
        host.ConfigureServices(services => 
        {
            // Command-specific service registrations go here
            services.AddSingleton(_ =>
            {
                var mb = new ModelBinder<GreeterOptions>
                { EnforceExplicitBinding = true };
                mb.BindMemberFromValue(o => o.Name, cliNameOption);
                return mb;
            });
            services.AddOptions<GreeterOptions>().BindCommandLine();
        });
    }),
};
cliRootCommand.Add(cliNameOption);

CliConfiguration cliConfig = new(cliRootCommand);
cliConfig.UseHost(Host.CreateDefaultBuilder, host =>
{
    host.ConfigureServices(services =>
    {
        // Common service registrations go here
    });
});
return await cliConfig.InvokeAsync(args)
    .ConfigureAwait(continueOnCapturedContext: false);