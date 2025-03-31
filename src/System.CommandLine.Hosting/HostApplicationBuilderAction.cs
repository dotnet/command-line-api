#if NET8_0_OR_GREATER
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace System.CommandLine.Hosting;

public class HostApplicationBuilderAction() : HostingAction()
{
    private new readonly Func<string[], HostApplicationBuilder>? _createHostBuilder;
    public new Action<HostApplicationBuilder>? ConfigureHost { get; set; }

    protected override IHostBuilder CreateHostBuiderCore(string[] args)
    {
        var hostBuilder = _createHostBuilder?.Invoke(args) ??
            new HostApplicationBuilder(args);
        return new HostApplicationBuilderWrapper(hostBuilder);
    }

    protected override void ConfigureHostBuilder(IHostBuilder hostBuilder)
    {
        base.ConfigureHostBuilder(hostBuilder);
        ConfigureHost?.Invoke(GetHostApplicationBuilder(hostBuilder));
    }

    private static HostApplicationBuilder GetHostApplicationBuilder(
        IHostBuilder hostBuilder
        )
    {
        return (HostApplicationBuilder)hostBuilder
            .Properties[typeof(HostApplicationBuilder)];
    }

    private class HostApplicationBuilderWrapper(
        HostApplicationBuilder hostApplicationBuilder
        ) : IHostBuilder
    {
        private Action? _useServiceProviderFactoryAction;
        private object? _configureServiceProviderBuilderAction;

        public HostBuilderContext Context { get; } = new(
            ((IHostApplicationBuilder)hostApplicationBuilder).Properties
            )
        {
            Configuration = hostApplicationBuilder.Configuration,
            HostingEnvironment = hostApplicationBuilder.Environment,
            Properties =
            { { typeof(HostApplicationBuilder), hostApplicationBuilder } }
        };

        public IDictionary<object, object> Properties => 
            ((IHostApplicationBuilder)hostApplicationBuilder).Properties;

        public IHost Build()
        {
            _useServiceProviderFactoryAction?.Invoke();
            return hostApplicationBuilder.Build();
        }

        public IHostBuilder ConfigureHostConfiguration(
            Action<IConfigurationBuilder> configureDelegate
            )
        {
            configureDelegate?.Invoke(hostApplicationBuilder.Configuration);
            return this;
        }

        public IHostBuilder ConfigureAppConfiguration(
            Action<HostBuilderContext, IConfigurationBuilder> configureDelegate
            )
        {
            SynchronizeContext();
            configureDelegate?.Invoke(
                Context, 
                hostApplicationBuilder.Configuration
                );
            SynchronizeContext();
            return this;
        }

        public IHostBuilder ConfigureServices(
            Action<HostBuilderContext, IServiceCollection> configureDelegate
            )
        {
            SynchronizeContext();
            configureDelegate?.Invoke(Context, hostApplicationBuilder.Services);
            SynchronizeContext();
            return this;
        }

        IHostBuilder IHostBuilder.UseServiceProviderFactory<TContainerBuilder>(
            IServiceProviderFactory<TContainerBuilder> factory
            )
        {
            _useServiceProviderFactoryAction = () =>
            {
                Action<TContainerBuilder>? configureDelegate = null;
                if (_configureServiceProviderBuilderAction is Action<HostBuilderContext, TContainerBuilder> configureDelegateWithContext)
                {
                    configureDelegate = builder => 
                    {
                        SynchronizeContext();
                        configureDelegateWithContext(Context, builder);
                        SynchronizeContext();
                    };
                }
                hostApplicationBuilder.ConfigureContainer(factory, configureDelegate);
            };
            return this;
        }

        IHostBuilder IHostBuilder.UseServiceProviderFactory<TContainerBuilder>(
            Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory
            )
        {
            _useServiceProviderFactoryAction = () =>
            {
                Action<TContainerBuilder>? configureDelegate = null;
                if (_configureServiceProviderBuilderAction is Action<HostBuilderContext, TContainerBuilder> configureDelegateWithContext)
                {
                    configureDelegate = builder => 
                    {
                        SynchronizeContext();
                        configureDelegateWithContext(Context, builder);
                        SynchronizeContext();
                    };
                }
                var factoryInstance = factory(Context);
                hostApplicationBuilder.ConfigureContainer(factoryInstance, configureDelegate);
            };
            return this;
        }

        IHostBuilder IHostBuilder.ConfigureContainer<TContainerBuilder>(
            Action<HostBuilderContext, TContainerBuilder> configureDelegate
            )
        {
            _configureServiceProviderBuilderAction = configureDelegate;
            return this;
        }

        private void SynchronizeContext()
        {
            Context.Configuration = hostApplicationBuilder.Configuration;
            Context.HostingEnvironment = hostApplicationBuilder.Environment;
        }
    }
}
#endif