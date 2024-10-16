#if NET8_0_OR_GREATER
using System.Collections.Generic;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace System.CommandLine.Hosting;

internal class HostApplicationBuilderAsIHostBuilder : IHostBuilder
{
    private readonly HostBuilderContext hostBuilderContext;
    private readonly HostApplicationBuilder appBuilder;
    private readonly Dictionary<object, object> properties;

    public IDictionary<object, object> Properties { get; }

    public HostApplicationBuilderAsIHostBuilder(
        HostApplicationBuilder appBuilder
        )
    {
        ArgumentNullException.ThrowIfNull(appBuilder);
        this.appBuilder = appBuilder;
        properties = new();
        hostBuilderContext = new(properties)
        {
            Configuration = appBuilder.Configuration,
            HostingEnvironment = appBuilder.Environment,
        };
    }

    public IHostBuilder ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate)
    {
        configureDelegate?.Invoke(appBuilder.Configuration);
        return this;
    }

    public IHostBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate)
    {
        configureDelegate?.Invoke(
            hostBuilderContext,
            appBuilder.Configuration
        );
        return this;
    }

    public IHostBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
    {
        configureDelegate?.Invoke(
            hostBuilderContext, 
            appBuilder.Services
        );
        return this;
    }

    public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory) where TContainerBuilder : notnull
    {
        throw new NotImplementedException();
    }

    public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory) where TContainerBuilder : notnull
    {
        throw new NotImplementedException();
    }

    public IHostBuilder ConfigureContainer<TContainerBuilder>(Action<HostBuilderContext, TContainerBuilder> configureDelegate)
    {
        throw new NotImplementedException();
    }

    public IHost Build() => appBuilder.Build();

    public static implicit operator HostApplicationBuilder(HostApplicationBuilderAsIHostBuilder wrapper)
    {
        ArgumentNullException.ThrowIfNull(wrapper);
        return wrapper.appBuilder;
    }

    public static explicit operator HostApplicationBuilderAsIHostBuilder(HostApplicationBuilder appBuilder) =>
        new(appBuilder);

    public override bool Equals(object obj) => obj switch
    {
        HostApplicationBuilder appBuilder => ReferenceEquals(this.appBuilder, appBuilder),
        HostApplicationBuilderAsIHostBuilder wrapper => ReferenceEquals(appBuilder, wrapper.appBuilder),
        _ => ReferenceEquals(this, obj)
    };

    public override int GetHashCode() => appBuilder.GetHashCode();
}
#endif