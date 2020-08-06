using System.CommandLine.Binding;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace System.CommandLine.Localization
{
    public static class LocalizationExtensions
    {
        public static CommandLineBuilder UseLocalization(
            this CommandLineBuilder builder, Type? resourceSource = null)
        {
            _ = builder ?? throw new ArgumentNullException(nameof(builder));

            builder.UseMiddleware((context, next) =>
            {
                var binding = context.BindingContext;

                binding.AddService(serviceProvider =>
                {
                    ILoggerFactory? loggerFactory = null;
                    // If using Generic Host integration
                    if (GetDynamicLoadedIHostInstance(serviceProvider) is { Interface: Type iHostType, Instance: object iHostInstance })
                    {
                        const BindingFlags getProperty = BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty;
                        var hostedServices = iHostType.InvokeMember(
                            "Services", getProperty, Type.DefaultBinder,
                            iHostInstance, null);
                        if (hostedServices is IServiceProvider hostedServiceProvider)
                        {
                            if (hostedServiceProvider.GetService<IStringLocalizerFactory>() is { } hostedLocalizer)
                                return hostedLocalizer;

                            // Extract logger factory if possible
                            loggerFactory = hostedServiceProvider.GetService<ILoggerFactory>();
                        }
                    }

                    // Construct default localizer
                    var options = serviceProvider.GetService<IOptions<LocalizationOptions>>() ??
                        Options.Create(new LocalizationOptions());
                    loggerFactory ??= serviceProvider.GetService<ILoggerFactory>() ??
                        Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance;
                    return new ResourceManagerStringLocalizerFactory(options, loggerFactory);

                    static (Type? Interface, object? Instance) GetDynamicLoadedIHostInstance(IServiceProvider serviceProvider)
                    {
                        var hostingAbstractionAsm = Assembly.Load("Microsoft.Extensions.Hosting.Abstractions");
                        if (hostingAbstractionAsm is null)
                            return default;
                        var iHostType = Type.GetType(@"Microsoft.Extensions.Hosting.IHost, Microsoft.Extensions.Hosting.Abstractions");
                        if (iHostType is null)
                            return default;
                        var iHostInstance = serviceProvider.GetService(iHostType);
                        return (iHostType, iHostInstance);
                    }
                });

                return next(context);
            }, MiddlewareOrder.ExceptionHandler);
            builder.UseHelpBuilder(ctx =>
            {
                var binder = new ModelBinder<LocalizedHelpBuilderFactory>();
                if (!(binder.CreateInstance(ctx) is LocalizedHelpBuilderFactory helpFactory))
                    throw new InvalidOperationException("Unable to resolve a localized help builder instance from the binding context.");
                return helpFactory.CreateHelpBuilder(resourceSource);
            });

            return builder;
        }
    }
}
