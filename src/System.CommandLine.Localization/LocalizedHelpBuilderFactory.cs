using System.CommandLine.Help;
using System.Reflection;
using Microsoft.Extensions.Localization;

namespace System.CommandLine.Localization
{
    internal class LocalizedHelpBuilderFactory
    {
        private readonly IStringLocalizerFactory localizerFactory;
        private readonly IConsole console;
        private readonly int? columnGutter;
        private readonly int? indentationSize;
        private readonly int? maxWidth;

        public LocalizedHelpBuilderFactory(
            IStringLocalizerFactory localizerFactory, IConsole console,
            int? columnGutter = null, int? indentationSize = null,
            int? maxWidth = null) : base()
        {
            this.localizerFactory = localizerFactory 
                ?? throw new ArgumentNullException(nameof(localizerFactory));
            this.console = console;
            this.columnGutter = columnGutter;
            this.indentationSize = indentationSize;
            this.maxWidth = maxWidth;
        }

        internal IHelpBuilder CreateHelpBuilder(Type? resourceSource = null)
        {
            if (resourceSource is null)
            {
                resourceSource = Assembly.GetEntryAssembly().EntryPoint.DeclaringType;
            }
            return new LocalizedHelpBuilder(localizerFactory.Create(resourceSource),
                console, columnGutter, indentationSize, maxWidth);
        }
    }
}