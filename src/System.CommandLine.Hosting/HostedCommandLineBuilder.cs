using System.Collections.Generic;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


namespace System.CommandLine.Hosting
{
    public interface ICommand
    {
        Task RunAsync(CancellationToken cancellationToken = default);
    }


    public class HostedCommandLineBuilder : CommandLineBuilder
    {
        private readonly List<Action<IServiceCollection>> _serviceConfigurations;


        public HostedCommandLineBuilder()
        {
            _serviceConfigurations = new List<Action<IServiceCollection>>();
        }


        public IReadOnlyList<Action<IServiceCollection>> ServiceConfigurations => _serviceConfigurations;


        public ICommandHandler BuildHandler<TCommand>()
            where TCommand : class, ICommand
        {
            _serviceConfigurations.Add(services => { services.AddSingleton<TCommand>(); });

            return CommandHandler.Create<IHost, CancellationToken>(async (host, cancellationToken) =>
            {
                var command = host.Services.GetRequiredService<TCommand>();

                await command.RunAsync(cancellationToken);
            });
        }
    }


    public static class CommandLineExtensions
    {
        public static IServiceCollection AddCommands(
            this IServiceCollection services,
            HostedCommandLineBuilder builder)
        {
            foreach (var configureService in builder.ServiceConfigurations)
            {
                configureService(services);
            }

            return services;
        }
    }
}
